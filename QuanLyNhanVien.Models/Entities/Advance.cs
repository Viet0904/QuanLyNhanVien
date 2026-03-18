namespace QuanLyNhanVien.Models.Entities
{
    public class Advance
    {
        public int AdvanceId { get; set; }
        public int EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime AdvanceDate { get; set; } = DateTime.Now;
        public int Month { get; set; } = DateTime.Now.Month;
        public int Year { get; set; } = DateTime.Now.Year;
        public string Status { get; set; } = "Pending";  // Pending, Approved, Deducted
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // JOIN fields
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
    }
}
