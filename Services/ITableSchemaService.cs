using FtpBulkInsert.Models;

namespace FtpBulkInsert.Services
{
    public interface ITableSchemaService
    {
        Task<List<ColumnInfo>> AnalyzeCsvStructureAsync(string filePath, int sampleRows = 100);
        Task CreateTableAsync(string tableName, List<ColumnInfo> columns);
    }
}
