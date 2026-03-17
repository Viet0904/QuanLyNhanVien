using System;

namespace QuanLyNhanVien.Models.Entities
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public string? ManagerName { get; set; }
        public string? ParentName { get; set; }
        public int EmployeeCount { get; set; }
    }
}
