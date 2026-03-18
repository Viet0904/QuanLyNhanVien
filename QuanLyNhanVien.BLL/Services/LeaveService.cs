using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.DTOs;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Services
{
    public class LeaveService
    {
        private readonly LeaveRequestRepository _leaveRepo;

        public LeaveService(LeaveRequestRepository leaveRepo)
        {
            _leaveRepo = leaveRepo;
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

            // Tính số ngày (chỉ tính weekday)
            req.TotalDays = CalculateWorkingDays(req.StartDate, req.EndDate);
            req.Status = "Pending";

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
            if (leave.Status != "Pending")
                return (false, $"Đơn đã ở trạng thái '{leave.Status}', không thể duyệt.");

            await _leaveRepo.UpdateStatusAsync(leaveId, "Approved", approvedBy);
            return (true, $"Đã duyệt đơn nghỉ phép của {leave.EmployeeName}.");
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
        /// Tính số ngày làm việc (bỏ qua T7, CN)
        /// </summary>
        private decimal CalculateWorkingDays(DateTime start, DateTime end)
        {
            int count = 0;
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    count++;
            }
            return count;
        }
    }
}
