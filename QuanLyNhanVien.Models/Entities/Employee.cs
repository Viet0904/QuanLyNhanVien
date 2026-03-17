using System;

namespace QuanLyNhanVien.Models.Entities
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? IdentityNo { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public byte[]? Photo { get; set; }
        public int DepartmentId { get; set; }
        public int PositionId { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public string? TaxCode { get; set; }
        public string? InsuranceNo { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal SalaryCoefficient { get; set; } = 1.0m;
        public int NumberOfDependents { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation (for display)
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
    }
}
