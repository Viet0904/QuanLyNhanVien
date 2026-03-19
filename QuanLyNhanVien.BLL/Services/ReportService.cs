using QuanLyNhanVien.DAL.Repositories;

namespace QuanLyNhanVien.BLL.Services
{
    public class ReportService
    {
        private readonly ReportRepository _reportRepo;

        public ReportService(ReportRepository reportRepo)
        {
            _reportRepo = reportRepo;
        }

        // Dashboard
        public async Task<(int TotalEmployees, int TotalDepts, int NewThisMonth, int TotalPositions)> GetDashboardStatsAsync()
            => await _reportRepo.GetDashboardStatsAsync();

        public async Task<IEnumerable<(string DeptName, int Count)>> GetDepartmentDistributionAsync()
            => await _reportRepo.GetDepartmentDistributionAsync();

        public async Task<IEnumerable<(int Month, decimal TotalNet, int RecordCount)>> GetMonthlySalarySummaryAsync(int year)
            => await _reportRepo.GetMonthlySalarySummaryAsync(year);

        public async Task<IEnumerable<dynamic>> GetTopSalaryAsync(int month, int year, int top = 5)
            => await _reportRepo.GetTopSalaryAsync(month, year, top);

        public async Task<IEnumerable<(string Status, int Count)>> GetLeaveStatisticsAsync(int year)
            => await _reportRepo.GetLeaveStatisticsAsync(year);

        public async Task<IEnumerable<dynamic>> GetAttendanceSummaryAsync(int month, int year)
            => await _reportRepo.GetAttendanceSummaryAsync(month, year);

        // Exports
        public async Task<IEnumerable<dynamic>> GetEmployeeReportAsync(int? deptId = null)
            => await _reportRepo.GetEmployeeReportAsync(deptId);

        public async Task<IEnumerable<dynamic>> GetSalaryReportAsync(int month, int year, int? deptId = null)
            => await _reportRepo.GetSalaryReportAsync(month, year, deptId);

        // Báo cáo tần suất đi muộn
        public async Task<IEnumerable<dynamic>> GetLateFrequencyReportAsync(int month, int year)
            => await _reportRepo.GetLateFrequencyReportAsync(month, year);

        // MISS-6: Báo cáo BHXH phần doanh nghiệp
        public async Task<IEnumerable<dynamic>> GetEmployerInsuranceReportAsync(int month, int year)
            => await _reportRepo.GetEmployerInsuranceReportAsync(month, year);
    }
}
