namespace FtpBulkInsert.Models
{
    public class BulkInsertResult
    {
        public string TableName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int RowsInserted { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class ProcessResult
    {
        public bool Success { get; set; }
        public List<BulkInsertResult> Results { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public int TotalFilesProcessed { get; set; }
        public int TotalRowsInserted { get; set; }
    }
}
