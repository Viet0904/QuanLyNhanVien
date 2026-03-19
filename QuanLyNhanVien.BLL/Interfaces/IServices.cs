using QuanLyNhanVien.Models.DTOs;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Interfaces
{
    /// <summary>
    /// ARCH-1: Interface cho SalaryService — DI Pattern cho unit testing
    /// </summary>
    public interface ISalaryService
    {
        // Config
        Task<IEnumerable<SalaryConfig>> GetAllConfigsAsync();
        Task<(bool Ok, string Msg)> SaveConfigAsync(SalaryConfig config);

        // Tính lương
        Task<SalaryRecord> CalculateForEmployeeAsync(Employee emp, int month, int year);
        Task<(bool Ok, string Msg, int Count)> CalculateMonthlyAsync(int month, int year, int? deptId = null);

        // Duyệt
        Task<(bool Ok, string Msg)> ApproveAllAsync(int month, int year, int approvedByUserId);
        Task<(bool Ok, string Msg)> ApproveAsync(long salaryId, int approvedByUserId);
        Task<(bool Ok, string Msg)> UnapproveAsync(long salaryId);
    }

    /// <summary>
    /// ARCH-1: Interface cho LeaveService
    /// </summary>
    public interface ILeaveService
    {
        Task<(bool Success, string Message, int LeaveId)> CreateAsync(LeaveRequest req);
        Task<(bool Success, string Message)> ApproveAsync(int leaveId, int approvedBy);
        Task<(bool Success, string Message)> RejectAsync(int leaveId, int rejectedBy);
        Task<PaginationResult<LeaveRequest>> GetListAsync(int? empId = null, string? status = null, int page = 1, int pageSize = 50);
    }

    /// <summary>
    /// ARCH-1: Interface cho AttendanceService
    /// </summary>
    public interface IAttendanceService
    {
        Task<IEnumerable<AttendanceRecord>> GetDailyAsync(DateTime date, int? deptId = null);
        Task<IEnumerable<AttendanceRecord>> GetMonthlyAsync(int month, int year, int? deptId = null);
        Task<(bool Success, string Message)> AddAsync(AttendanceRecord record);
        Task<(bool Success, string Message)> UpdateAsync(AttendanceRecord record);
        Task<(bool Success, string Message)> DeleteAsync(long recordId);
    }

    /// <summary>
    /// ARCH-1: Interface cho ContractService
    /// </summary>
    public interface IContractService
    {
        Task<IEnumerable<Contract>> GetAllAsync(int? employeeId = null, bool? isActive = null);
        Task<IEnumerable<Contract>> GetExpiringAsync(int daysAhead = 30);
        Task<IEnumerable<Contract>> GetExpiredActiveContractsAsync();
        Task<(bool Ok, string Msg, int Count)> DeactivateExpiredEmployeesAsync();
        Task<(bool Ok, string Msg)> CreateAsync(Contract contract);
        Task<(bool Ok, string Msg)> UpdateAsync(Contract contract);
        Task<(bool Ok, string Msg)> DeleteAsync(int contractId);
    }
}
