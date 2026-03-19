namespace QuanLyNhanVien.Models.Common
{
    /// <summary>
    /// ARCH-4: Status constants thay thế magic strings
    /// Tất cả status dùng trong hệ thống được định nghĩa tại đây
    /// </summary>
    public static class StatusConstants
    {
        // === Attendance Status ===
        public const string Present = "Present";
        public const string Late = "Late";
        public const string EarlyLeave = "EarlyLeave";
        public const string LateAndEarlyLeave = "LateAndEarlyLeave";
        public const string OnLeave = "OnLeave";
        public const string UnpaidLeave = "UnpaidLeave";
        public const string Absent = "Absent";

        // === Leave Request Status ===
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";

        // === Salary Status ===
        public const string Draft = "Draft";
        // Approved đã có ở trên

        // === Advance Status ===
        public const string Deducted = "Deducted";
        // Pending, Approved đã có ở trên

        // === Leave Type ===
        public const string LeaveAnnual = "Annual";
        public const string LeaveAnnualVi = "PHEP_NAM";
        public const string LeaveSick = "Sick";
        public const string LeaveUnpaid = "Unpaid";
        public const string LeaveUnpaidVi = "NGHI_KL";
        public const string LeaveMaternity = "Maternity";
    }
}
