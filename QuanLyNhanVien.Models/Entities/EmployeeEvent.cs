namespace QuanLyNhanVien.Models.Entities
{
    public class EmployeeEvent
    {
        public int EventId { get; set; }
        public int EmployeeId { get; set; }
        public string EventType { get; set; } = string.Empty;  // REWARD, DISCIPLINE, PROMOTION, TRANSFER
        public DateTime EventDate { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }      // Tiền thưởng/phạt (nếu có)
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // JOIN fields
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
    }
}
