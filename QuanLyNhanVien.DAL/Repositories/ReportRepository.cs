using Dapper;
using QuanLyNhanVien.DAL.Context;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class ReportRepository : BaseRepository
    {
        public ReportRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        /// <summary>
        /// Tổng quan: NV active, phòng ban, NV mới tháng này
        /// </summary>
        public async Task<(int TotalEmployees, int TotalDepts, int NewThisMonth, int TotalPositions)> GetDashboardStatsAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            var totalEmp = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Employees WHERE IsActive = 1");
            var totalDept = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Departments WHERE IsActive = 1");
            var totalPos = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Positions WHERE IsActive = 1");
            var newMonth = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Employees WHERE IsActive = 1 AND MONTH(HireDate) = @M AND YEAR(HireDate) = @Y",
                new { M = DateTime.Now.Month, Y = DateTime.Now.Year });
            return (totalEmp, totalDept, newMonth, totalPos);
        }

        /// <summary>
        /// Số NV theo phòng ban
        /// </summary>
        public async Task<IEnumerable<(string DeptName, int Count)>> GetDepartmentDistributionAsync()
        {
            var sql = @"SELECT d.DepartmentName AS DeptName, COUNT(e.EmployeeId) AS [Count]
                        FROM Departments d
                        LEFT JOIN Employees e ON d.DepartmentId = e.DepartmentId AND e.IsActive = 1
                        WHERE d.IsActive = 1
                        GROUP BY d.DepartmentName
                        ORDER BY [Count] DESC";
            using var conn = _dbFactory.CreateConnection();
            var results = await conn.QueryAsync(sql);
            return results.Select(r => ((string)r.DeptName, (int)r.Count));
        }

        /// <summary>
        /// Tổng chi lương theo tháng trong năm
        /// </summary>
        public async Task<IEnumerable<(int Month, decimal TotalNet, int RecordCount)>> GetMonthlySalarySummaryAsync(int year)
        {
            var sql = @"SELECT Month, SUM(NetSalary) AS TotalNet, COUNT(*) AS RecordCount
                        FROM SalaryRecords WHERE Year = @Year AND Status = 'Approved'
                        GROUP BY Month ORDER BY Month";
            using var conn = _dbFactory.CreateConnection();
            var results = await conn.QueryAsync(sql, new { Year = year });
            return results.Select(r => ((int)r.Month, (decimal)r.TotalNet, (int)r.RecordCount));
        }

        /// <summary>
        /// Top NV lương cao nhất
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetTopSalaryAsync(int month, int year, int top = 5)
        {
            var sql = @"SELECT TOP(@Top) e.EmployeeCode, e.FullName AS EmployeeName, d.DepartmentName,
                          sr.GrossIncome, sr.NetSalary, sr.WorkingDays
                        FROM SalaryRecords sr
                        INNER JOIN Employees e ON sr.EmployeeId = e.EmployeeId
                        LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                        WHERE sr.Month = @Month AND sr.Year = @Year
                        ORDER BY sr.NetSalary DESC";
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync(sql, new { Top = top, Month = month, Year = year });
        }

        /// <summary>
        /// Thống kê nghỉ phép theo trạng thái
        /// </summary>
        public async Task<IEnumerable<(string Status, int Count)>> GetLeaveStatisticsAsync(int year)
        {
            var sql = @"SELECT Status, COUNT(*) AS [Count]
                        FROM LeaveRequests
                        WHERE YEAR(StartDate) = @Year
                        GROUP BY Status";
            using var conn = _dbFactory.CreateConnection();
            var results = await conn.QueryAsync(sql, new { Year = year });
            return results.Select(r => ((string)r.Status, (int)r.Count));
        }

        /// <summary>
        /// Tóm tắt chấm công tháng
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetAttendanceSummaryAsync(int month, int year)
        {
            var sql = @"SELECT e.EmployeeCode, e.FullName,
                          COUNT(CASE WHEN a.Status = 'Present' THEN 1 END) AS PresentDays,
                          COUNT(CASE WHEN a.Status = 'Absent' THEN 1 END) AS AbsentDays,
                          COUNT(CASE WHEN a.Status = 'Late' THEN 1 END) AS LateDays,
                          SUM(ISNULL(a.OvertimeHours, 0)) AS TotalOT
                        FROM Employees e
                        LEFT JOIN AttendanceRecords a ON e.EmployeeId = a.EmployeeId
                          AND MONTH(a.WorkDate) = @Month AND YEAR(a.WorkDate) = @Year
                        WHERE e.IsActive = 1
                        GROUP BY e.EmployeeCode, e.FullName
                        HAVING COUNT(a.RecordId) > 0
                        ORDER BY e.FullName";
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync(sql, new { Month = month, Year = year });
        }

        /// <summary>
        /// Báo cáo danh sách nhân viên đầy đủ
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetEmployeeReportAsync(int? deptId = null)
        {
            var where = deptId.HasValue ? "AND e.DepartmentId = @DeptId" : "";
            var sql = $@"SELECT e.EmployeeCode, e.FullName, e.Gender, e.DateOfBirth, e.Phone, e.Email,
                           d.DepartmentName, p.PositionName, e.BasicSalary, e.SalaryCoefficient,
                           e.HireDate, CASE WHEN e.IsActive=1 THEN N'Đang làm' ELSE N'Nghỉ việc' END AS Status
                         FROM Employees e
                         LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                         LEFT JOIN Positions p ON e.PositionId = p.PositionId
                         WHERE e.IsActive = 1 {where}
                         ORDER BY d.DepartmentName, e.FullName";
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync(sql, new { DeptId = deptId });
        }

        /// <summary>
        /// Báo cáo lương tháng chi tiết
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetSalaryReportAsync(int month, int year, int? deptId = null)
        {
            var where = deptId.HasValue ? "AND sr.EmployeeId IN (SELECT EmployeeId FROM Employees WHERE DepartmentId = @DeptId)" : "";
            var sql = $@"SELECT sr.EmployeeCode, sr.EmployeeName, sr.DepartmentName,
                           sr.WorkingDays, sr.StandardDays, sr.BasicSalary, sr.SalaryCoefficient,
                           sr.PositionAllowance, sr.OvertimePay, sr.GrossIncome,
                           sr.SocialInsurance, sr.HealthInsurance, sr.UnemploymentInsurance,
                           sr.PersonalIncomeTax, sr.NetSalary, sr.Status
                         FROM SalaryRecords sr
                         WHERE sr.Month = @Month AND sr.Year = @Year {where}
                         ORDER BY sr.DepartmentName, sr.EmployeeName";
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync(sql, new { Month = month, Year = year, DeptId = deptId });
        }

        /// <summary>
        /// Báo cáo NV mới / nghỉ việc theo khoảng thời gian
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetNewTerminatedReportAsync(int month, int year)
        {
            var sql = @"SELECT e.EmployeeCode, e.FullName, d.DepartmentName,
                           e.HireDate, e.TerminationDate,
                           CASE WHEN e.IsActive=1 THEN N'Đang làm' ELSE N'Nghỉ việc' END AS Status
                         FROM Employees e
                         LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                         WHERE (MONTH(e.HireDate)=@Month AND YEAR(e.HireDate)=@Year)
                            OR (e.TerminationDate IS NOT NULL AND MONTH(e.TerminationDate)=@Month AND YEAR(e.TerminationDate)=@Year)
                         ORDER BY e.HireDate";
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync(sql, new { Month = month, Year = year });
        }

        /// <summary>
        /// Sinh nhật trong tháng
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetBirthdayReportAsync(int month)
        {
            var sql = @"SELECT e.EmployeeCode, e.FullName, e.DateOfBirth, d.DepartmentName,
                           DAY(e.DateOfBirth) AS BirthDay
                         FROM Employees e
                         LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                         WHERE e.IsActive = 1 AND MONTH(e.DateOfBirth) = @Month
                         ORDER BY DAY(e.DateOfBirth)";
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync(sql, new { Month = month });
        }

        /// <summary>
        /// Báo cáo BHXH
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetInsuranceReportAsync(int month, int year)
        {
            var sql = @"SELECT sr.EmployeeCode, sr.EmployeeName, sr.DepartmentName,
                           sr.BasicSalary, sr.SalaryCoefficient,
                           sr.SocialInsurance, sr.HealthInsurance, sr.UnemploymentInsurance,
                           (sr.SocialInsurance + sr.HealthInsurance + sr.UnemploymentInsurance) AS TotalInsurance
                         FROM SalaryRecords sr
                         WHERE sr.Month = @Month AND sr.Year = @Year
                         ORDER BY sr.DepartmentName, sr.EmployeeName";
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync(sql, new { Month = month, Year = year });
        }

        /// <summary>
        /// MISS-6: Báo cáo BHXH phần doanh nghiệp (17.5% BHXH + 3% BHYT + 1% BHTN)
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetEmployerInsuranceReportAsync(int month, int year)
        {
            var sql = @"SELECT sr.EmployeeCode, sr.EmployeeName, sr.DepartmentName,
                           sr.BasicSalary, sr.SalaryCoefficient,
                           (sr.BasicSalary * sr.SalaryCoefficient) AS InsuranceBase,
                           -- Phần người lao động
                           sr.SocialInsurance AS EmpBHXH,
                           sr.HealthInsurance AS EmpBHYT,
                           sr.UnemploymentInsurance AS EmpBHTN,
                           (sr.SocialInsurance + sr.HealthInsurance + sr.UnemploymentInsurance) AS EmpTotal,
                           -- Phần doanh nghiệp (17.5% + 3% + 1%)
                           ROUND((sr.BasicSalary * sr.SalaryCoefficient) * 0.175, 0) AS CorpBHXH,
                           ROUND((sr.BasicSalary * sr.SalaryCoefficient) * 0.03, 0) AS CorpBHYT,
                           ROUND((sr.BasicSalary * sr.SalaryCoefficient) * 0.01, 0) AS CorpBHTN,
                           ROUND((sr.BasicSalary * sr.SalaryCoefficient) * 0.215, 0) AS CorpTotal,
                           -- Tổng cả hai bên
                           (sr.SocialInsurance + sr.HealthInsurance + sr.UnemploymentInsurance) +
                           ROUND((sr.BasicSalary * sr.SalaryCoefficient) * 0.215, 0) AS GrandTotal
                         FROM SalaryRecords sr
                         WHERE sr.Month = @Month AND sr.Year = @Year
                         ORDER BY sr.DepartmentName, sr.EmployeeName";
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync(sql, new { Month = month, Year = year });
        }

        /// <summary>
        /// Báo cáo thuế TNCN
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetTaxReportAsync(int month, int year)
        {
            var sql = @"SELECT sr.EmployeeCode, sr.EmployeeName, sr.DepartmentName,
                           sr.GrossIncome, sr.PersonalIncomeTax, sr.NetSalary,
                           e.NumberOfDependents
                         FROM SalaryRecords sr
                         INNER JOIN Employees e ON sr.EmployeeId = e.EmployeeId
                         WHERE sr.Month = @Month AND sr.Year = @Year AND sr.PersonalIncomeTax > 0
                         ORDER BY sr.PersonalIncomeTax DESC";
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync(sql, new { Month = month, Year = year });
        }

        /// <summary>
        /// Biến động nhân sự (tỷ lệ vào/ra theo tháng trong năm)
        /// </summary>
        public async Task<IEnumerable<(int Month, int NewHires, int Terminations)>> GetTurnoverAsync(int year)
        {
            var sql = @"SELECT m.Month,
                           ISNULL(h.NewHires, 0) AS NewHires,
                           ISNULL(t.Terminations, 0) AS Terminations
                         FROM (SELECT 1 AS Month UNION SELECT 2 UNION SELECT 3 UNION SELECT 4
                               UNION SELECT 5 UNION SELECT 6 UNION SELECT 7 UNION SELECT 8
                               UNION SELECT 9 UNION SELECT 10 UNION SELECT 11 UNION SELECT 12) m
                         LEFT JOIN (SELECT MONTH(HireDate) AS Month, COUNT(*) AS NewHires
                                    FROM Employees WHERE YEAR(HireDate) = @Year GROUP BY MONTH(HireDate)) h ON m.Month = h.Month
                         LEFT JOIN (SELECT MONTH(TerminationDate) AS Month, COUNT(*) AS Terminations
                                    FROM Employees WHERE TerminationDate IS NOT NULL AND YEAR(TerminationDate) = @Year
                                    GROUP BY MONTH(TerminationDate)) t ON m.Month = t.Month
                         ORDER BY m.Month";
            using var conn = _dbFactory.CreateConnection();
            var results = await conn.QueryAsync(sql, new { Year = year });
            return results.Select(r => ((int)r.Month, (int)r.NewHires, (int)r.Terminations));
        }

        /// <summary>
        /// Báo cáo tần suất đi muộn/về sớm theo tháng
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetLateFrequencyReportAsync(int month, int year)
        {
            var sql = @"SELECT e.EmployeeCode, e.FullName AS EmployeeName, d.DepartmentName,
                           COUNT(*) AS TotalWorkDays,
                           COUNT(CASE WHEN a.[Status] = 'Late' THEN 1 END) AS LateDays,
                           COUNT(CASE WHEN a.[Status] = 'EarlyLeave' THEN 1 END) AS EarlyLeaveDays,
                           COUNT(CASE WHEN a.[Status] IN ('Late','EarlyLeave') THEN 1 END) AS TotalViolations,
                           CAST(
                             ROUND(
                               COUNT(CASE WHEN a.[Status] IN ('Late','EarlyLeave') THEN 1 END) * 100.0 / NULLIF(COUNT(*), 0),
                               1
                             ) AS DECIMAL(5,1)
                           ) AS ViolationPercent
                         FROM AttendanceRecords a
                         INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
                         LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                         WHERE MONTH(a.WorkDate) = @Month AND YEAR(a.WorkDate) = @Year
                           AND e.IsActive = 1
                         GROUP BY e.EmployeeCode, e.FullName, d.DepartmentName
                         HAVING COUNT(CASE WHEN a.[Status] IN ('Late','EarlyLeave') THEN 1 END) > 0
                         ORDER BY TotalViolations DESC, e.FullName";
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync(sql, new { Month = month, Year = year });
        }
    }
}
