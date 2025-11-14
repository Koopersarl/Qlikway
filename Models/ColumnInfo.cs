namespace FtpBulkInsert.Models
{
    public class ColumnInfo
    {
        public string Name { get; set; } = string.Empty;
        public string SqlType { get; set; } = string.Empty;
        public int MaxLength { get; set; }
        public bool IsNullable { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
    }

    public enum DetectedType
    {
        Integer,
        Decimal,
        Date,
        DateTime,
        Boolean,
        String
    }
}
