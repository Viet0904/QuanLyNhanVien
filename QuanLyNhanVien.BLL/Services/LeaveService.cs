using QuanLyNhanVien.BLL.Interfaces;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.DTOs;
using QuanLyNhanVien.Models.Entities;
using static QuanLyNhanVien.Models.Common.StatusConstants;

namespace QuanLyNhanVien.BLL.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly LeaveRequestRepository _leaveRepo;
        private readonly HolidayRepository _holidayRepo;
        private readonly AttendanceRepository _attendanceRepo;

        public LeaveService(LeaveRequestRepository leaveRepo, HolidayRepository holidayRepo, AttendanceRepository attendanceRepo)
        {
            _leaveRepo = leaveRepo;
            _holidayRepo = holidayRepo;
            _attendanceRepo = attendanceRepo;
        }

        /// <summary>
        /// Danh sách đơn phép (phân trang, filter)
        /// </summary>
        public async Task<PaginationResult<LeaveRequest>> GetListAsync(
            int? empId = null, string? status = null, int page = 1, int pageSize = 50)
        {
            return await _leaveRepo.GetAllAsync(empId, status, page, pageSize);
        }

        /// <summary>
        /// Chi tiết đơn phép
        /// </summary>
        public async Task<LeaveRequest?> GetByIdAsync(int leaveId)
        {
            return await _leaveRepo.GetByIdAsync(leaveId);
        }

        /// <summary>
        /// Tạo đơn nghỉ phép mới
        /// </summary>
        public async Task<(bool Success, string Message, int LeaveId)> CreateAsync(LeaveRequest req)
        {
            // Validate
            if (req.EmployeeId <= 0)
                return (false, "Vui lòng chọn nhân viên.", 0);
            if (req.StartDate > req.EndDate)
                return (false, "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.", 0);
            if (string.IsNullOrWhiteSpace(req.LeaveType))
                return (false, "Vui lòng chọn loại nghỉ phép.", 0);

            // Kiểm tra trùng khoảng ngày
            var overlap = await _leaveRepo.HasOverlappingLeaveAsync(req.EmployeeId, req.StartDate, req.EndDate);
            if (overlap)
                return (false, "Nhân viên đã có đơn nghỉ phép trùng khoảng ngày này.", 0);

            // Tính số ngày (bỏ T7, CN và ngày lễ từ DB)
            req.TotalDays = await CalculateWorkingDaysAsync(req.StartDate, req.EndDate);

            // MISS-1: Kiểm tra giới hạn phép năm (chỉ cho loại Annual)
            if (req.LeaveType == LeaveAnnual || req.LeaveType == LeaveAnnualVi)
            {
                var year = req.StartDate.Year;
                var usedDays = await _leaveRepo.GetUsedLeaveDaysAsync(req.EmployeeId, year);
                // Mặc định 12 ngày theo Luật Lao động VN (Điều 113)
                const decimal maxAnnualLeave = 12;
                var remaining = maxAnnualLeave - usedDays;

                if (req.TotalDays > remaining)
                {
                    return (false, 
                        $"Vượt quá số ngày phép năm! Đã dùng: {usedDays}/{maxAnnualLeave} ngày. Còn lại: {remaining} ngày.", 
                        0);
                }
            }

            req.Status = Pending;

            var id = await _leaveRepo.InsertAsync(req);
            return (true, $"Tạo đơn nghỉ phép thành công! ({req.TotalDays} ngày)", id);
        }

        /// <summary>
        /// Duyệt đơn nghỉ phép
        /// </summary>
        public async Task<(bool Success, string Message)> ApproveAsync(int leaveId, int approvedBy)
        {
            var leave = await _leaveRepo.GetByIdAsync(leaveId);
            if (leave == null)
                return (false, "Không tìm thấy đơn nghỉ phép.");
            if (leave.Status != Pending)
                return (false, $"Đơn đã ở trạng thái '{leave.Status}', không thể duyệt.");

            await _leaveRepo.UpdateStatusAsync(leaveId, Approved, approvedBy);

            // MISS-8: Tự động tạo bản ghi chấm công cho các ngày nghỉ đã duyệt
            await AutoCreateAttendanceForLeaveAsync(leave);

            return (true, $"Đã duyệt đơn nghỉ phép của {leave.EmployeeName} và tự động cập nhật chấm công.");
        }

        /// <summary>
        /// MISS-8: Tạo bản ghi chấm công tự động khi nghỉ phép được duyệt
        /// </summary>
        private async Task AutoCreateAttendanceForLeaveAsync(LeaveRequest leave)
        {
            var holidayDates = await _holidayRepo.GetHolidayDatesAsync(leave.StartDate, leave.EndDate);

            // Xác định status chấm công dựa trên loại nghỉ phép
            var status = leave.LeaveType switch
            {
                LeaveUnpaid or LeaveUnpaidVi => UnpaidLeave,
                _ => OnLeave
            };

            for (var d = leave.StartDate; d <= leave.EndDate; d = d.AddDays(1))
            {
                // Bỏ qua T7, CN, ngày lễ
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                    continue;
                if (holidayDates.Contains(d.Date))
                    continue;

                // Kiểm tra xem đã có bản ghi chấm công cho ngày này chưa
                var existing = await _attendanceRepo.GetByEmployeeAndDateAsync(leave.EmployeeId, d);
                if (existing != null)
                    continue;

                // Tạo bản ghi chấm công tự động
                var record = new AttendanceRecord
                {
                    EmployeeId = leave.EmployeeId,
                    WorkDate = d,
                    Status = status,
                    Notes = $"Tự động từ đơn phép #{leave.LeaveId}"
                };
                await _attendanceRepo.InsertAsync(record);
            }
        }

        /// <summary>
        /// Từ chối đơn nghỉ phép
        /// </summary>
        public async Task<(bool Success, string Message)> RejectAsync(int leaveId, int rejectedBy)
        {
            var leave = await _leaveRepo.GetByIdAsync(leaveId);
            if (leave == null)
                return (false, "Không tìm thấy đơn nghỉ phép.");
            if (leave.Status != "Pending")
                return (false, $"Đơn đã ở trạng thái '{leave.Status}', không thể từ chối.");

            await _leaveRepo.UpdateStatusAsync(leaveId, "Rejected", rejectedBy);
            return (true, $"Đã từ chối đơn nghỉ phép của {leave.EmployeeName}.");
        }

        /// <summary>
        /// Xóa đơn phép (chỉ khi Pending)
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteAsync(int leaveId)
        {
            var leave = await _leaveRepo.GetByIdAsync(leaveId);
            if (leave == null)
                return (false, "Không tìm thấy đơn nghỉ phép.");
            if (leave.Status != "Pending")
                return (false, "Chỉ có thể xóa đơn ở trạng thái 'Đang chờ duyệt'.");

            await _leaveRepo.DeleteAsync(leaveId);
            return (true, "Đã xóa đơn nghỉ phép.");
        }

        /// <summary>
        /// Lấy số ngày phép đã dùng trong năm
        /// </summary>
        public async Task<decimal> GetUsedLeaveDaysAsync(int empId, int year)
        {
            return await _leaveRepo.GetUsedLeaveDaysAsync(empId, year);
        }

        /// <summary>
        /// Tính số ngày làm việc (bỏ T7, CN, và ngày lễ từ DB)
        /// </summary>
        private async Task<decimal> CalculateWorkingDaysAsync(DateTime start, DateTime end)
        {
            // Lấy ngày lễ từ DB thay vì hardcode
            var holidayDates = await _holidayRepo.GetHolidayDatesAsync(start, end);

            int count = 0;
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                    continue;
                if (holidayDates.Contains(d.Date))
                    continue;
                count++;
            }
            return count;
        }
    }
}
