using FtpBulkInsert.Models;

namespace FtpBulkInsert.Services
{
    public interface IBulkInsertService
    {
        Task<BulkInsertResult> ProcessFileAsync(string fileName, string localFilePath);
    }
}
