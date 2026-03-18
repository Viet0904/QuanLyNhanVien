using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class AdvanceRepository : BaseRepository
    {
        public AdvanceRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        public async Task<IEnumerable<Advance>> GetAllAsync(int? month = null, int? year = null)
        {
            var sql = @"SELECT a.*, e.FullName AS EmployeeName, e.EmployeeCode
                        FROM Advances a
                        INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
                        WHERE 1=1 ";
            if (month.HasValue) sql += " AND a.Month = @Month";
            if (year.HasValue) sql += " AND a.Year = @Year";
            sql += " ORDER BY a.AdvanceDate DESC";
            return await QuerySqlAsync<Advance>(sql, new { Month = month, Year = year });
        }

        public async Task<decimal> GetTotalAdvanceAsync(int employeeId, int month, int year)
        {
            var sql = @"SELECT ISNULL(SUM(Amount), 0) FROM Advances
                        WHERE EmployeeId = @EmpId AND Month = @Month AND Year = @Year AND Status IN ('Approved','Deducted')";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<decimal>(sql, new { EmpId = employeeId, Month = month, Year = year });
        }

        public async Task<int> InsertAsync(Advance advance)
        {
            var sql = @"INSERT INTO Advances (EmployeeId, Amount, AdvanceDate, Month, Year, Status, Notes)
                        VALUES (@EmployeeId, @Amount, @AdvanceDate, @Month, @Year, @Status, @Notes);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, advance);
        }

        public async Task UpdateAsync(Advance advance)
        {
            var sql = @"UPDATE Advances SET Amount = @Amount, AdvanceDate = @AdvanceDate,
                        Month = @Month, Year = @Year, Status = @Status, Notes = @Notes
                        WHERE AdvanceId = @AdvanceId";
            await ExecuteSqlAsync(sql, advance);
        }

        public async Task DeleteAsync(int advanceId)
        {
            var sql = "DELETE FROM Advances WHERE AdvanceId = @Id";
            await ExecuteSqlAsync(sql, new { Id = advanceId });
        }
    }
}
