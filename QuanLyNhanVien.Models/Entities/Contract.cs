namespace QuanLyNhanVien.Models.Entities
{
    public class Contract
    {
        public int ContractId { get; set; }
        public int EmployeeId { get; set; }
        public string ContractCode { get; set; } = string.Empty;
        public string ContractType { get; set; } = string.Empty;   // CT_TV, CT_XDTH, CT_KXDTH, CT_KHOAN
        public DateTime SignDate { get; set; } = DateTime.Now;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }
        public decimal ContractSalary { get; set; }  // Mức lương ghi trên HĐ
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // JOIN fields
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? ContractTypeName { get; set; }
    }
}
