using FtpBulkInsert.Models;
using Microsoft.Data.SqlClient;

namespace FtpBulkInsert.Services
{
    public class LogService : ILogService
    {
        private readonly string _connectionString;
        private readonly ILogger<LogService> _logger;

        public LogService(IConfiguration configuration, ILogger<LogService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("ConnectionStrings:DefaultConnection");
            _logger = logger;
        }

        public async Task LogUploadAsync(BulkInsertResult result)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO [UploadLog]
                        ([TableName], [FileName], [UploadDate], [RowsInserted], [Success], [ErrorMessage], [DurationMs])
                    VALUES
                        (@TableName, @FileName, @UploadDate, @RowsInserted, @Success, @ErrorMessage, @DurationMs)";

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@TableName", result.TableName);
                command.Parameters.AddWithValue("@FileName", result.FileName);
                command.Parameters.AddWithValue("@UploadDate", DateTime.Now);
                command.Parameters.AddWithValue("@RowsInserted", result.RowsInserted);
                command.Parameters.AddWithValue("@Success", result.Success);
                command.Parameters.AddWithValue("@ErrorMessage", (object?)result.ErrorMessage ?? DBNull.Value);
                command.Parameters.AddWithValue("@DurationMs", (long)result.Duration.TotalMilliseconds);

                await command.ExecuteNonQueryAsync();

                _logger.LogInformation($"Log d'upload enregistré pour {result.TableName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de l'enregistrement du log pour {result.TableName}");
                // Ne pas relancer l'exception pour ne pas bloquer le processus principal
            }
        }
    }
}
