using System;

namespace QuanLyNhanVien.Models.Entities
{
    public class AuditLog
    {
        public long LogId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string? RecordId { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IPAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public string? Username { get; set; }
    }
}
