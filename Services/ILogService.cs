using FtpBulkInsert.Models;

namespace FtpBulkInsert.Services
{
    public interface ILogService
    {
        Task LogUploadAsync(BulkInsertResult result);
    }
}
