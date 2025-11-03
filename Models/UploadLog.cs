namespace FtpBulkInsert.Models
{
    public class UploadLog
    {
        public int Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public int RowsInserted { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
