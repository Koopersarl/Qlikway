using FtpBulkInsert.Models;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace FtpBulkInsert.Services
{
    public class BulkInsertService : IBulkInsertService
    {
        private readonly string _connectionString;
        private readonly ILogger<BulkInsertService> _logger;

        public BulkInsertService(IConfiguration configuration, ILogger<BulkInsertService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("ConnectionStrings:DefaultConnection");
            _logger = logger;
        }

        public async Task<BulkInsertResult> ProcessFileAsync(string fileName, string localFilePath)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BulkInsertResult
            {
                FileName = fileName,
                TableName = Path.GetFileNameWithoutExtension(fileName)
            };

            try
            {
                _logger.LogInformation($"Traitement du fichier {fileName} pour la table {result.TableName}");

                // Vérifier que le fichier existe
                if (!File.Exists(localFilePath))
                {
                    throw new FileNotFoundException($"Le fichier {localFilePath} n'existe pas");
                }

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Vérifier que la table existe
                var tableExists = await CheckTableExistsAsync(connection, result.TableName);
                if (!tableExists)
                {
                    throw new Exception($"La table {result.TableName} n'existe pas dans la base de données");
                }

                // Vider la table
                _logger.LogInformation($"Vidage de la table {result.TableName}");
                await TruncateTableAsync(connection, result.TableName);

                // Compter les lignes avant insertion (pour info)
                var lineCount = await CountLinesInFileAsync(localFilePath);
                _logger.LogInformation($"Le fichier contient environ {lineCount} lignes (en-tête inclus)");

                // Effectuer le BULK INSERT
                _logger.LogInformation($"Début du BULK INSERT pour {result.TableName}");
                var rowsInserted = await ExecuteBulkInsertAsync(connection, result.TableName, localFilePath);

                result.RowsInserted = rowsInserted;
                result.Success = true;

                _logger.LogInformation($"BULK INSERT terminé: {rowsInserted} lignes insérées dans {result.TableName}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, $"Erreur lors du traitement du fichier {fileName}");
            }
            finally
            {
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;

                // Nettoyer le fichier temporaire
                try
                {
                    if (File.Exists(localFilePath))
                    {
                        File.Delete(localFilePath);
                        _logger.LogInformation($"Fichier temporaire supprimé: {localFilePath}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Impossible de supprimer le fichier temporaire {localFilePath}");
                }
            }

            return result;
        }

        private async Task<bool> CheckTableExistsAsync(SqlConnection connection, string tableName)
        {
            var sql = @"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_NAME = @TableName";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TableName", tableName);

            var count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }

        private async Task TruncateTableAsync(SqlConnection connection, string tableName)
        {
            // Utiliser un nom de table sécurisé (échapper les crochets)
            var safeTableName = $"[{tableName.Replace("]", "]]")}]";
            var sql = $"TRUNCATE TABLE {safeTableName}";

            using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

        private async Task<int> ExecuteBulkInsertAsync(SqlConnection connection, string tableName, string filePath)
        {
            // Utiliser un nom de table sécurisé
            var safeTableName = $"[{tableName.Replace("]", "]]")}]";

            // Échapper le chemin du fichier pour SQL
            var escapedPath = filePath.Replace("'", "''");

            var sql = $@"
                BULK INSERT {safeTableName}
                FROM '{escapedPath}'
                WITH (
                    FIRSTROW = 2,
                    FIELDTERMINATOR = ',',
                    ROWTERMINATOR = '\n',
                    TABLOCK,
                    KEEPNULLS,
                    FORMAT = 'CSV'
                )";

            using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = 300; // 5 minutes timeout
            await command.ExecuteNonQueryAsync();

            // Compter le nombre de lignes insérées
            var countSql = $"SELECT COUNT(*) FROM {safeTableName}";
            using var countCommand = new SqlCommand(countSql, connection);
            var count = (int)await countCommand.ExecuteScalarAsync();

            return count;
        }

        private async Task<int> CountLinesInFileAsync(string filePath)
        {
            var count = 0;
            using var reader = new StreamReader(filePath);
            while (await reader.ReadLineAsync() != null)
            {
                count++;
            }
            return count;
        }
    }
}
