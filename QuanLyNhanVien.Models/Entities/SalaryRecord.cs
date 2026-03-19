using System;

namespace QuanLyNhanVien.Models.Entities
{
    public class SalaryConfig
    {
        public int ConfigId { get; set; }
        public string ConfigCode { get; set; } = string.Empty;
        public string ConfigName { get; set; } = string.Empty;
        public decimal ConfigValue { get; set; }
        public string ConfigType { get; set; } = "Percent"; // Percent / Amount / Days
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SalaryRecord
    {
        public long SalaryId { get; set; }
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal WorkingDays { get; set; }
        public decimal StandardDays { get; set; } = 26;
        public decimal BasicSalary { get; set; }
        public decimal SalaryCoefficient { get; set; }
        public decimal PositionAllowance { get; set; }
        public decimal OtherAllowance { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal GrossIncome { get; set; }
        public decimal SocialInsurance { get; set; }
        public decimal HealthInsurance { get; set; }
        public decimal UnemploymentInsurance { get; set; }
        public decimal PersonalDeduction { get; set; }
        public decimal DependentDeduction { get; set; }
        public decimal TaxableIncome { get; set; }
        public decimal PersonalIncomeTax { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = "Draft";
        public int? ApprovedBy { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
    }
}
