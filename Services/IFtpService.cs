namespace FtpBulkInsert.Services
{
    public interface IFtpService
    {
        Task<List<string>> ListCsvFilesAsync();
        Task<string> DownloadFileAsync(string fileName);
        Task DeleteFileAsync(string fileName);
    }
}
