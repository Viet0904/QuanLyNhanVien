using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class ContractRepository : BaseRepository
    {
        public ContractRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        public virtual async Task<IEnumerable<Contract>> GetAllAsync(int? employeeId = null, bool? isActive = null)
        {
            var sql = @"SELECT c.*, e.FullName AS EmployeeName, e.EmployeeCode,
                        ci.ItemName AS ContractTypeName
                        FROM Contracts c
                        INNER JOIN Employees e ON c.EmployeeId = e.EmployeeId
                        LEFT JOIN CategoryItems ci ON c.ContractType = ci.ItemCode
                        WHERE 1=1 ";
            if (employeeId.HasValue) sql += " AND c.EmployeeId = @EmployeeId";
            if (isActive.HasValue) sql += " AND c.IsActive = @IsActive";
            sql += " ORDER BY c.StartDate DESC";
            return await QuerySqlAsync<Contract>(sql, new { EmployeeId = employeeId, IsActive = isActive });
        }

        public virtual async Task<Contract?> GetByIdAsync(int contractId)
        {
            var sql = @"SELECT c.*, e.FullName AS EmployeeName, e.EmployeeCode
                        FROM Contracts c
                        INNER JOIN Employees e ON c.EmployeeId = e.EmployeeId
                        WHERE c.ContractId = @Id";
            return (await QuerySqlAsync<Contract>(sql, new { Id = contractId })).FirstOrDefault();
        }

        public virtual async Task<int> InsertAsync(Contract contract)
        {
            var sql = @"INSERT INTO Contracts (EmployeeId, ContractCode, ContractType, SignDate, StartDate, EndDate, ContractSalary, Notes, IsActive)
                        VALUES (@EmployeeId, @ContractCode, @ContractType, @SignDate, @StartDate, @EndDate, @ContractSalary, @Notes, @IsActive);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, contract);
        }

        public virtual async Task UpdateAsync(Contract contract)
        {
            var sql = @"UPDATE Contracts SET ContractType = @ContractType, SignDate = @SignDate,
                        StartDate = @StartDate, EndDate = @EndDate, ContractSalary = @ContractSalary,
                        Notes = @Notes, IsActive = @IsActive
                        WHERE ContractId = @ContractId";
            await ExecuteSqlAsync(sql, contract);
        }

        public virtual async Task DeleteAsync(int contractId)
        {
            var sql = "UPDATE Contracts SET IsActive = 0 WHERE ContractId = @Id";
            await ExecuteSqlAsync(sql, new { Id = contractId });
        }

        /// <summary>
        /// Lấy HĐ sắp hết hạn (trong vòng N ngày)
        /// </summary>
        public virtual async Task<IEnumerable<Contract>> GetExpiringAsync(int daysAhead = 30)
        {
            var sql = @"SELECT c.*, e.FullName AS EmployeeName, e.EmployeeCode,
                        ci.ItemName AS ContractTypeName
                        FROM Contracts c
                        INNER JOIN Employees e ON c.EmployeeId = e.EmployeeId
                        LEFT JOIN CategoryItems ci ON c.ContractType = ci.ItemCode
                        WHERE c.IsActive = 1 AND c.EndDate IS NOT NULL
                        AND c.EndDate BETWEEN GETDATE() AND DATEADD(DAY, @Days, GETDATE())
                        ORDER BY c.EndDate";
            return await QuerySqlAsync<Contract>(sql, new { Days = daysAhead });
        }

        /// <summary>
        /// MISS-4: Lấy HĐ đã hết hạn nhưng NV vẫn active
        /// </summary>
        public virtual async Task<IEnumerable<Contract>> GetExpiredActiveAsync()
        {
            var sql = @"SELECT c.*, e.FullName AS EmployeeName, e.EmployeeCode
                        FROM Contracts c
                        INNER JOIN Employees e ON c.EmployeeId = e.EmployeeId
                        WHERE c.IsActive = 1 AND c.EndDate IS NOT NULL
                        AND c.EndDate < GETDATE()
                        AND e.IsActive = 1
                        AND NOT EXISTS (
                            SELECT 1 FROM Contracts c2 
                            WHERE c2.EmployeeId = c.EmployeeId 
                            AND c2.IsActive = 1 AND c2.ContractId != c.ContractId
                            AND (c2.EndDate IS NULL OR c2.EndDate >= GETDATE())
                        )
                        ORDER BY c.EndDate";
            return await QuerySqlAsync<Contract>(sql);
        }
    }
}
