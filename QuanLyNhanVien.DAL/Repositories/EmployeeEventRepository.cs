using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class EmployeeEventRepository : BaseRepository
    {
        public EmployeeEventRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        public async Task<IEnumerable<EmployeeEvent>> GetAllAsync(int? employeeId = null, string? eventType = null)
        {
            var sql = @"SELECT ev.*, e.FullName AS EmployeeName, e.EmployeeCode
                        FROM EmployeeEvents ev
                        INNER JOIN Employees e ON ev.EmployeeId = e.EmployeeId
                        WHERE 1=1 ";
            if (employeeId.HasValue) sql += " AND ev.EmployeeId = @EmployeeId";
            if (!string.IsNullOrEmpty(eventType)) sql += " AND ev.EventType = @EventType";
            sql += " ORDER BY ev.EventDate DESC";
            return await QuerySqlAsync<EmployeeEvent>(sql, new { EmployeeId = employeeId, EventType = eventType });
        }

        public async Task<int> InsertAsync(EmployeeEvent ev)
        {
            var sql = @"INSERT INTO EmployeeEvents (EmployeeId, EventType, EventDate, [Description], Amount, CreatedBy)
                        VALUES (@EmployeeId, @EventType, @EventDate, @Description, @Amount, @CreatedBy);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, ev);
        }

        public async Task UpdateAsync(EmployeeEvent ev)
        {
            var sql = @"UPDATE EmployeeEvents SET EventType = @EventType, EventDate = @EventDate,
                        [Description] = @Description, Amount = @Amount WHERE EventId = @EventId";
            await ExecuteSqlAsync(sql, ev);
        }

        public async Task DeleteAsync(int eventId)
        {
            var sql = "DELETE FROM EmployeeEvents WHERE EventId = @Id";
            await ExecuteSqlAsync(sql, new { Id = eventId });
        }
    }
}
