namespace QuanLyNhanVien.Models.Entities
{
    public class Holiday
    {
        public int HolidayId { get; set; }
        public string HolidayName { get; set; } = string.Empty;
        public DateTime HolidayDate { get; set; }
        public int Year { get; set; }
        public bool IsRecurring { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }
}
