using System.Collections.Generic;

namespace QuanLyNhanVien.Models.DTOs
{
    /// <summary>
    /// Kết quả phân trang
    /// </summary>
    public class PaginationResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }

    public class DashboardDto
    {
        public int TotalEmployees { get; set; }
        public int NewEmployeesThisMonth { get; set; }
        public int TerminatedThisMonth { get; set; }
        public decimal AttendanceRate { get; set; }
        public List<DepartmentStatDto> DepartmentStats { get; set; } = new();
    }

    public class DepartmentStatDto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
    }
}
