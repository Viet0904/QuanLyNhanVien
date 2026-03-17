using System;

namespace QuanLyNhanVien.Models.Entities
{
    public class AttendanceRecord
    {
        public long RecordId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime WorkDate { get; set; }
        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }
        public string? ShiftType { get; set; }
        public string Status { get; set; } = "Present";
        public decimal OvertimeHours { get; set; }
        public string? Notes { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
    }
}
