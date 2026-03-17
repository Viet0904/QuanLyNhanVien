namespace QuanLyNhanVien.Models.Entities
{
    public class Position
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public int Level { get; set; }
        public decimal AllowanceAmount { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
