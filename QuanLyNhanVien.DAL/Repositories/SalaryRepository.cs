using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class SalaryRepository : BaseRepository
    {
        public SalaryRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        // ===== SALARY CONFIGS =====

        public async Task<IEnumerable<SalaryConfig>> GetAllConfigsAsync()
        {
            var sql = "SELECT * FROM SalaryConfigs WHERE IsActive = 1 ORDER BY ConfigCode";
            return await QuerySqlAsync<SalaryConfig>(sql);
        }

        public async Task<SalaryConfig?> GetConfigByCodeAsync(string code)
        {
            var sql = "SELECT TOP 1 * FROM SalaryConfigs WHERE ConfigCode = @Code AND IsActive = 1";
            var result = await QuerySqlAsync<SalaryConfig>(sql, new { Code = code });
            return result.FirstOrDefault();
        }

        public async Task UpsertConfigAsync(SalaryConfig config)
        {
            if (config.ConfigId > 0)
            {
                var sql = @"UPDATE SalaryConfigs SET ConfigName = @ConfigName, ConfigValue = @ConfigValue,
                            ConfigType = @ConfigType, EffectiveFrom = @EffectiveFrom, EffectiveTo = @EffectiveTo
                            WHERE ConfigId = @ConfigId";
                await ExecuteSqlAsync(sql, config);
            }
            else
            {
                var sql = @"INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom, EffectiveTo)
                            VALUES (@ConfigCode, @ConfigName, @ConfigValue, @ConfigType, @EffectiveFrom, @EffectiveTo)";
                await ExecuteSqlAsync(sql, config);
            }
        }

        // ===== SALARY RECORDS =====

        public async Task<IEnumerable<SalaryRecord>> GetSalaryRecordsAsync(int month, int year, int? deptId = null)
        {
            var where = "WHERE s.[Month] = @Month AND s.[Year] = @Year";
            if (deptId.HasValue)
                where += " AND e.DepartmentId = @DeptId";

            var sql = $@"SELECT s.*, e.FullName AS EmployeeName, e.EmployeeCode, d.DepartmentName
                         FROM SalaryRecords s
                         INNER JOIN Employees e ON s.EmployeeId = e.EmployeeId
                         LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                         {where}
                         ORDER BY e.EmployeeCode";
            return await QuerySqlAsync<SalaryRecord>(sql, new { Month = month, Year = year, DeptId = deptId });
        }

        public async Task<SalaryRecord?> GetByIdAsync(long salaryId)
        {
            var sql = @"SELECT s.*, e.FullName AS EmployeeName, e.EmployeeCode, d.DepartmentName
                        FROM SalaryRecords s
                        INNER JOIN Employees e ON s.EmployeeId = e.EmployeeId
                        LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                        WHERE s.SalaryId = @SalaryId";
            var result = await QuerySqlAsync<SalaryRecord>(sql, new { SalaryId = salaryId });
            return result.FirstOrDefault();
        }

        public async Task<SalaryRecord?> GetByEmployeeMonthAsync(int empId, int month, int year)
        {
            var sql = "SELECT * FROM SalaryRecords WHERE EmployeeId = @EmpId AND [Month] = @Month AND [Year] = @Year";
            var result = await QuerySqlAsync<SalaryRecord>(sql, new { EmpId = empId, Month = month, Year = year });
            return result.FirstOrDefault();
        }

        public async Task<long> InsertAsync(SalaryRecord record)
        {
            var sql = @"INSERT INTO SalaryRecords 
                        (EmployeeId, [Month], [Year], WorkingDays, StandardDays, BasicSalary, SalaryCoefficient,
                         PositionAllowance, OtherAllowance, OvertimePay, GrossIncome,
                         SocialInsurance, HealthInsurance, UnemploymentInsurance,
                         PersonalDeduction, DependentDeduction, TaxableIncome, PersonalIncomeTax,
                         OtherDeductions, NetSalary, [Status], Notes)
                        VALUES 
                        (@EmployeeId, @Month, @Year, @WorkingDays, @StandardDays, @BasicSalary, @SalaryCoefficient,
                         @PositionAllowance, @OtherAllowance, @OvertimePay, @GrossIncome,
                         @SocialInsurance, @HealthInsurance, @UnemploymentInsurance,
                         @PersonalDeduction, @DependentDeduction, @TaxableIncome, @PersonalIncomeTax,
                         @OtherDeductions, @NetSalary, @Status, @Notes);
                        SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<long>(sql, record);
        }

        public async Task<int> DeleteDraftByMonthAsync(int month, int year)
        {
            var sql = "DELETE FROM SalaryRecords WHERE [Month] = @Month AND [Year] = @Year AND [Status] = 'Draft'";
            return await ExecuteSqlAsync(sql, new { Month = month, Year = year });
        }

        public async Task UpdateStatusAsync(long salaryId, string status, int? approvedBy)
        {
            var sql = "UPDATE SalaryRecords SET [Status] = @Status, ApprovedBy = @ApprovedBy WHERE SalaryId = @SalaryId";
            await ExecuteSqlAsync(sql, new { SalaryId = salaryId, Status = status, ApprovedBy = approvedBy });
        }

        public async Task BulkUpdateStatusAsync(int month, int year, string status, int approvedBy)
        {
            var sql = @"UPDATE SalaryRecords SET [Status] = @Status, ApprovedBy = @ApprovedBy 
                        WHERE [Month] = @Month AND [Year] = @Year AND [Status] = 'Draft'";
            await ExecuteSqlAsync(sql, new { Month = month, Year = year, Status = status, ApprovedBy = approvedBy });
        }

        public async Task<IEnumerable<SalaryRecord>> GetHistoryAsync(int empId)
        {
            var sql = @"SELECT s.*, e.FullName AS EmployeeName, e.EmployeeCode, d.DepartmentName
                        FROM SalaryRecords s
                        INNER JOIN Employees e ON s.EmployeeId = e.EmployeeId
                        LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                        WHERE s.EmployeeId = @EmpId
                        ORDER BY s.[Year] DESC, s.[Month] DESC";
            return await QuerySqlAsync<SalaryRecord>(sql, new { EmpId = empId });
        }
    }
}
