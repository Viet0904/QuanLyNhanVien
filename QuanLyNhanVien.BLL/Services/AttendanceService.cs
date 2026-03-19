using QuanLyNhanVien.BLL.Interfaces;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;
using static QuanLyNhanVien.Models.Common.StatusConstants;

namespace QuanLyNhanVien.BLL.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AttendanceRepository _attendanceRepo;
        private readonly EmployeeRepository _empRepo;

        public AttendanceService(AttendanceRepository attendanceRepo, EmployeeRepository empRepo)
        {
            _attendanceRepo = attendanceRepo;
            _empRepo = empRepo;
        }

        /// <summary>
        /// Lấy danh sách chấm công theo ngày
        /// </summary>
        public async Task<IEnumerable<AttendanceRecord>> GetDailyAsync(DateTime date, int? deptId = null)
        {
            return await _attendanceRepo.GetByDateAsync(date, deptId);
        }

        /// <summary>
        /// Lấy bảng chấm công tháng
        /// </summary>
        public async Task<IEnumerable<AttendanceRecord>> GetMonthlyAsync(int month, int year, int? deptId = null)
        {
            return await _attendanceRepo.GetMonthlyAsync(month, year, deptId);
        }

        /// <summary>
        /// Thêm bản ghi chấm công (1 NV, 1 ngày)
        /// </summary>
        public async Task<(bool Success, string Message)> AddAsync(AttendanceRecord record)
        {
            if (record.EmployeeId <= 0)
                return (false, "Vui lòng chọn nhân viên.");

            // Kiểm tra đã chấm công chưa
            var existing = await _attendanceRepo.GetByEmployeeAndDateAsync(record.EmployeeId, record.WorkDate);
            if (existing != null)
                return (false, "Nhân viên đã được chấm công ngày này rồi.");

            // Tự động xác định status (Late / EarlyLeave / Present)
            if (record.CheckIn.HasValue && !string.IsNullOrEmpty(record.ShiftType))
            {
                record.Status = DetermineStatus(record.CheckIn.Value, record.CheckOut, record.ShiftType);
            }

            // Tính overtime
            if (record.CheckIn.HasValue && record.CheckOut.HasValue)
            {
                record.OvertimeHours = CalculateOvertime(record.CheckIn.Value, record.CheckOut.Value, record.ShiftType);
            }

            await _attendanceRepo.InsertAsync(record);
            return (true, "Chấm công thành công!");
        }

        /// <summary>
        /// Cập nhật bản ghi chấm công
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateAsync(AttendanceRecord record)
        {
            // Tự động tính lại status và overtime
            if (record.CheckIn.HasValue && !string.IsNullOrEmpty(record.ShiftType))
            {
                record.Status = DetermineStatus(record.CheckIn.Value, record.CheckOut, record.ShiftType);
            }
            if (record.CheckIn.HasValue && record.CheckOut.HasValue)
            {
                record.OvertimeHours = CalculateOvertime(record.CheckIn.Value, record.CheckOut.Value, record.ShiftType);
            }

            await _attendanceRepo.UpdateAsync(record);
            return (true, "Cập nhật chấm công thành công!");
        }

        /// <summary>
        /// Xóa bản ghi chấm công
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteAsync(long recordId)
        {
            await _attendanceRepo.DeleteAsync(recordId);
            return (true, "Đã xóa bản ghi chấm công.");
        }

        /// <summary>
        /// Chấm công hàng loạt — chấm tất cả NV chưa chấm trong ngày
        /// </summary>
        public async Task<(bool Success, string Message, int Count)> BulkMarkAsync(
            DateTime workDate, int? departmentId, string shiftType, int createdBy)
        {
            var notChecked = (await _attendanceRepo.GetEmployeesNotCheckedAsync(workDate, departmentId)).ToList();
            if (!notChecked.Any())
                return (false, "Tất cả nhân viên đã được chấm công ngày này.", 0);

            var records = notChecked.Select(emp => new AttendanceRecord
            {
                EmployeeId = emp.EmployeeId,
                WorkDate = workDate,
                ShiftType = shiftType,
                Status = "Present",
                CreatedBy = createdBy
            }).ToList();

            var count = await _attendanceRepo.BulkInsertAsync(records);
            return (true, $"Đã chấm công hàng loạt cho {count} nhân viên.", count);
        }

        /// <summary>
        /// Chấm công hàng loạt nâng cao — cho phép chấm với giờ vào/ra cụ thể
        /// </summary>
        public async Task<(bool Success, string Message, int Count)> BulkInsertWithDetailsAsync(
            List<AttendanceRecord> records)
        {
            if (records == null || records.Count == 0)
                return (false, "Không có dữ liệu chấm công.", 0);

            // Auto-calculate status and overtime for each record
            foreach (var record in records)
            {
                if (record.CheckIn.HasValue && !string.IsNullOrEmpty(record.ShiftType))
                {
                    record.Status = DetermineStatus(record.CheckIn.Value, record.CheckOut, record.ShiftType);
                }
                if (record.CheckIn.HasValue && record.CheckOut.HasValue)
                {
                    record.OvertimeHours = CalculateOvertime(record.CheckIn.Value, record.CheckOut.Value, record.ShiftType);
                }
            }

            var count = await _attendanceRepo.BulkInsertAsync(records);
            return (true, $"Đã chấm công hàng loạt cho {count} nhân viên.", count);
        }

        /// <summary>
        /// Lấy danh sách NV chưa chấm công
        /// </summary>
        public async Task<IEnumerable<Employee>> GetUncheckedEmployeesAsync(DateTime date, int? deptId = null)
        {
            return await _attendanceRepo.GetEmployeesNotCheckedAsync(date, deptId);
        }

        /// <summary>
        /// Xác định trạng thái dựa trên giờ check-in, check-out và ca làm việc.
        /// Hỗ trợ: Present, Late, EarlyLeave
        /// </summary>
        private string DetermineStatus(TimeSpan checkIn, TimeSpan? checkOut, string shiftType)
        {
            // Lấy giờ bắt đầu ca
            var shiftStart = GetShiftStart(shiftType);
            // Lấy giờ kết thúc ca
            var shiftEnd = GetShiftEnd(shiftType);

            // Trễ nếu check-in sau giờ bắt đầu ca + 15 phút
            bool isLate = checkIn > shiftStart.Add(TimeSpan.FromMinutes(15));

            // Về sớm nếu check-out trước giờ kết thúc ca - 15 phút
            bool isEarlyLeave = false;
            if (checkOut.HasValue)
            {
                bool isNightShift = shiftType == "NIGHT";
                if (isNightShift)
                {
                    // Ca đêm: normalize checkout nếu vượt qua nửa đêm
                    var normalizedCheckOut = checkOut.Value < checkIn
                        ? checkOut.Value.Add(TimeSpan.FromHours(24))
                        : checkOut.Value;
                    var normalizedShiftEnd = shiftEnd.Add(TimeSpan.FromHours(24));
                    isEarlyLeave = normalizedCheckOut < normalizedShiftEnd.Subtract(TimeSpan.FromMinutes(15));
                }
                else
                {
                    isEarlyLeave = checkOut.Value < shiftEnd.Subtract(TimeSpan.FromMinutes(15));
                }
            }

            if (isLate && isEarlyLeave) return LateAndEarlyLeave;
            if (isLate) return Late;
            if (isEarlyLeave) return EarlyLeave;
            return Present;
        }

        /// <summary>
        /// Lấy giờ bắt đầu ca làm việc
        /// </summary>
        private static TimeSpan GetShiftStart(string shiftType)
        {
            return shiftType switch
            {
                "MORNING" or "FULLDAY" => new TimeSpan(7, 30, 0),
                "AFTERNOON" => new TimeSpan(13, 0, 0),
                "NIGHT" => new TimeSpan(22, 0, 0),
                _ => new TimeSpan(8, 0, 0)
            };
        }

        /// <summary>
        /// Lấy giờ kết thúc ca làm việc
        /// </summary>
        private static TimeSpan GetShiftEnd(string shiftType)
        {
            return shiftType switch
            {
                "MORNING" => new TimeSpan(11, 30, 0),
                "AFTERNOON" or "FULLDAY" => new TimeSpan(17, 0, 0),
                "NIGHT" => new TimeSpan(6, 0, 0), // hôm sau
                _ => new TimeSpan(17, 0, 0)
            };
        }

        /// <summary>
        /// Tính giờ tăng ca (xử lý cả ca đêm vượt qua 0:00)
        /// </summary>
        private decimal CalculateOvertime(TimeSpan checkIn, TimeSpan checkOut, string? shiftType)
        {
            var shiftEnd = GetShiftEnd(shiftType ?? "FULLDAY");
            bool isNightShift = shiftType == "NIGHT";

            if (isNightShift)
            {
                var normalizedCheckOut = checkOut < checkIn ? checkOut.Add(TimeSpan.FromHours(24)) : checkOut;
                var normalizedShiftEnd = shiftEnd.Add(TimeSpan.FromHours(24));

                if (normalizedCheckOut > normalizedShiftEnd)
                {
                    var overtime = normalizedCheckOut - normalizedShiftEnd;
                    return Math.Round((decimal)overtime.TotalHours, 2);
                }
            }
            else
            {
                if (checkOut > shiftEnd)
                {
                    var overtime = checkOut - shiftEnd;
                    return Math.Round((decimal)overtime.TotalHours, 2);
                }
            }

            return 0;
        }
    }
}
