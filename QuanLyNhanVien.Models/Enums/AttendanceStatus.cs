namespace QuanLyNhanVien.Models.Enums
{
    /// <summary>
    /// Trạng thái chấm công
    /// </summary>
    public enum AttendanceStatus
    {
        Present,            // Có mặt
        Late,               // Đi trễ
        EarlyLeave,         // Về sớm
        LateAndEarlyLeave,  // Vừa trễ vừa về sớm
        Absent,             // Vắng mặt
        OnLeave,            // Nghỉ phép
        UnpaidLeave,        // Nghỉ không lương
        Holiday             // Ngày lễ
    }
}
