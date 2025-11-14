using FtpBulkInsert.Models;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FtpBulkInsert.Services
{
    public class TableSchemaService : ITableSchemaService
    {
        private readonly string _connectionString;
        private readonly ILogger<TableSchemaService> _logger;

        public TableSchemaService(IConfiguration configuration, ILogger<TableSchemaService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("ConnectionStrings:DefaultConnection");
            _logger = logger;
        }

        public async Task<List<ColumnInfo>> AnalyzeCsvStructureAsync(string filePath, int sampleRows = 100)
        {
            _logger.LogInformation($"Analyse de la structure du fichier CSV: {filePath}");

            var columns = new List<ColumnInfo>();
            var columnSamples = new Dictionary<int, List<string>>();

            using var reader = new StreamReader(filePath);

            // Lire la première ligne (en-têtes)
            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(headerLine))
            {
                throw new Exception("Le fichier CSV est vide");
            }

            var headers = ParseCsvLine(headerLine);
            _logger.LogInformation($"Colonnes détectées: {string.Join(", ", headers)}");

            // Initialiser les listes d'échantillons
            for (int i = 0; i < headers.Length; i++)
            {
                columnSamples[i] = new List<string>();
            }

            // Lire les lignes d'échantillon
            int rowCount = 0;
            while (rowCount < sampleRows && !reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;

                var values = ParseCsvLine(line);
                for (int i = 0; i < Math.Min(values.Length, headers.Length); i++)
                {
                    columnSamples[i].Add(values[i]);
                }

                rowCount++;
            }

            _logger.LogInformation($"Analyse de {rowCount} lignes d'échantillon");

            // Analyser chaque colonne
            for (int i = 0; i < headers.Length; i++)
            {
                var columnName = SanitizeColumnName(headers[i]);
                var samples = columnSamples[i];

                var columnInfo = AnalyzeColumn(columnName, samples);
                columns.Add(columnInfo);

                _logger.LogInformation($"Colonne '{columnInfo.Name}': {columnInfo.SqlType}");
            }

            return columns;
        }

        private ColumnInfo AnalyzeColumn(string columnName, List<string> samples)
        {
            var columnInfo = new ColumnInfo
            {
                Name = columnName,
                IsNullable = samples.Any(s => string.IsNullOrWhiteSpace(s))
            };

            // Filtrer les valeurs vides pour l'analyse
            var nonEmptySamples = samples.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            if (nonEmptySamples.Count == 0)
            {
                // Toutes les valeurs sont vides/nulles
                columnInfo.SqlType = "NVARCHAR(255)";
                columnInfo.MaxLength = 255;
                return columnInfo;
            }

            // Détection du type
            var detectedType = DetectType(nonEmptySamples);

            switch (detectedType)
            {
                case DetectedType.Boolean:
                    columnInfo.SqlType = "BIT";
                    break;

                case DetectedType.Integer:
                    var maxIntValue = nonEmptySamples.Max(s => long.TryParse(s, out var val) ? Math.Abs(val) : 0);
                    if (maxIntValue <= 127)
                        columnInfo.SqlType = "TINYINT";
                    else if (maxIntValue <= 32767)
                        columnInfo.SqlType = "SMALLINT";
                    else if (maxIntValue <= 2147483647)
                        columnInfo.SqlType = "INT";
                    else
                        columnInfo.SqlType = "BIGINT";
                    break;

                case DetectedType.Decimal:
                    var (precision, scale) = AnalyzeDecimalPrecision(nonEmptySamples);
                    columnInfo.SqlType = $"DECIMAL({precision},{scale})";
                    columnInfo.Precision = precision;
                    columnInfo.Scale = scale;
                    break;

                case DetectedType.Date:
                    columnInfo.SqlType = "DATE";
                    break;

                case DetectedType.DateTime:
                    columnInfo.SqlType = "DATETIME2";
                    break;

                case DetectedType.String:
                default:
                    var maxLength = nonEmptySamples.Max(s => s.Length);

                    // Choisir la taille appropriée
                    if (maxLength <= 50)
                        columnInfo.SqlType = "NVARCHAR(50)";
                    else if (maxLength <= 100)
                        columnInfo.SqlType = "NVARCHAR(100)";
                    else if (maxLength <= 255)
                        columnInfo.SqlType = "NVARCHAR(255)";
                    else if (maxLength <= 500)
                        columnInfo.SqlType = "NVARCHAR(500)";
                    else if (maxLength <= 1000)
                        columnInfo.SqlType = "NVARCHAR(1000)";
                    else if (maxLength <= 4000)
                        columnInfo.SqlType = "NVARCHAR(4000)";
                    else
                        columnInfo.SqlType = "NVARCHAR(MAX)";

                    columnInfo.MaxLength = maxLength;
                    break;
            }

            return columnInfo;
        }

        private DetectedType DetectType(List<string> samples)
        {
            // Prendre un échantillon représentatif
            var sampleSize = Math.Min(samples.Count, 50);
            var testSamples = samples.Take(sampleSize).ToList();

            // Test Boolean (au moins 80% doivent être valides)
            var boolMatches = testSamples.Count(s => IsBooleanValue(s));
            if (boolMatches >= testSamples.Count * 0.8)
                return DetectedType.Boolean;

            // Test Integer (au moins 90% doivent être valides)
            var intMatches = testSamples.Count(s => long.TryParse(s, out _));
            if (intMatches >= testSamples.Count * 0.9)
                return DetectedType.Integer;

            // Test Decimal (au moins 90% doivent être valides)
            var decimalMatches = testSamples.Count(s => decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _));
            if (decimalMatches >= testSamples.Count * 0.9)
                return DetectedType.Decimal;

            // Test DateTime (au moins 80% doivent être valides)
            var dateTimeMatches = testSamples.Count(s => TryParseDateTime(s, out var dt) && dt.TimeOfDay != TimeSpan.Zero);
            if (dateTimeMatches >= testSamples.Count * 0.8)
                return DetectedType.DateTime;

            // Test Date (au moins 80% doivent être valides)
            var dateMatches = testSamples.Count(s => TryParseDateTime(s, out var dt) && dt.TimeOfDay == TimeSpan.Zero);
            if (dateMatches >= testSamples.Count * 0.8)
                return DetectedType.Date;

            // Par défaut: String
            return DetectedType.String;
        }

        private bool IsBooleanValue(string value)
        {
            var lower = value.Trim().ToLower();
            return lower == "true" || lower == "false" ||
                   lower == "1" || lower == "0" ||
                   lower == "yes" || lower == "no" ||
                   lower == "oui" || lower == "non";
        }

        private bool TryParseDateTime(string value, out DateTime result)
        {
            // Essayer plusieurs formats
            var formats = new[]
            {
                "yyyy-MM-dd",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd HH:mm:ss.fff",
                "dd/MM/yyyy",
                "dd/MM/yyyy HH:mm:ss",
                "MM/dd/yyyy",
                "MM/dd/yyyy HH:mm:ss",
                "yyyy/MM/dd",
                "yyyy/MM/dd HH:mm:ss"
            };

            return DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ||
                   DateTime.TryParse(value, out result);
        }

        private (int precision, int scale) AnalyzeDecimalPrecision(List<string> samples)
        {
            int maxIntegerDigits = 0;
            int maxDecimalDigits = 0;

            foreach (var sample in samples.Take(50))
            {
                if (decimal.TryParse(sample, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    var parts = sample.Replace(",", ".").Split('.');
                    var integerPart = parts[0].TrimStart('-', '+');
                    var decimalPart = parts.Length > 1 ? parts[1] : "";

                    maxIntegerDigits = Math.Max(maxIntegerDigits, integerPart.Length);
                    maxDecimalDigits = Math.Max(maxDecimalDigits, decimalPart.Length);
                }
            }

            // Limiter à la capacité de DECIMAL
            var precision = Math.Min(maxIntegerDigits + maxDecimalDigits, 38);
            var scale = Math.Min(maxDecimalDigits, precision - 1);

            // Valeurs par défaut si on n'a pas détecté de décimales
            if (precision == 0) precision = 18;
            if (scale == 0) scale = 2;

            return (precision, scale);
        }

        private string[] ParseCsvLine(string line)
        {
            // Parser CSV simple (gère les virgules entre guillemets)
            var values = new List<string>();
            var currentValue = "";
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(currentValue.Trim().Trim('"'));
                    currentValue = "";
                }
                else
                {
                    currentValue += c;
                }
            }

            values.Add(currentValue.Trim().Trim('"'));
            return values.ToArray();
        }

        private string SanitizeColumnName(string name)
        {
            // Nettoyer le nom de colonne
            var sanitized = name.Trim();

            // Remplacer les caractères invalides par des underscores
            sanitized = Regex.Replace(sanitized, @"[^\w\s]", "_");

            // Remplacer les espaces par des underscores
            sanitized = Regex.Replace(sanitized, @"\s+", "_");

            // Limiter la longueur
            if (sanitized.Length > 128)
                sanitized = sanitized.Substring(0, 128);

            // S'assurer que le nom commence par une lettre
            if (!char.IsLetter(sanitized[0]) && sanitized[0] != '_')
                sanitized = "_" + sanitized;

            return sanitized;
        }

        public async Task CreateTableAsync(string tableName, List<ColumnInfo> columns)
        {
            _logger.LogInformation($"Création de la table {tableName} avec {columns.Count} colonnes");

            var safeTableName = $"[{tableName.Replace("]", "]]")}]";

            var columnDefinitions = columns.Select(c =>
            {
                var nullable = c.IsNullable ? "NULL" : "NOT NULL";
                return $"    [{c.Name}] {c.SqlType} {nullable}";
            });

            var sql = $@"
CREATE TABLE {safeTableName} (
{string.Join($",{Environment.NewLine}", columnDefinitions)}
)";

            _logger.LogInformation($"SQL de création: {sql}");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();

            _logger.LogInformation($"Table {tableName} créée avec succès");
        }
    }
}
