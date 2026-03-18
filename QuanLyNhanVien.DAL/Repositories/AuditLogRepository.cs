using Dapper;
using QuanLyNhanVien.DAL.Context;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class AuditLogRepository : BaseRepository
    {
        public AuditLogRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        public async Task<IEnumerable<dynamic>> GetLogsAsync(DateTime? from = null, DateTime? to = null, int? userId = null, int limit = 200)
        {
            var where = "WHERE 1=1";
            if (from.HasValue) where += " AND al.CreatedAt >= @From";
            if (to.HasValue) where += " AND al.CreatedAt <= @To";
            if (userId.HasValue) where += " AND al.UserId = @UserId";

            var sql = $@"SELECT TOP(@Limit) al.LogId, u.Username, al.Action, al.TableName,
                           al.RecordId, al.OldValue, al.NewValue, al.IPAddress, al.CreatedAt
                         FROM AuditLogs al
                         LEFT JOIN Users u ON al.UserId = u.UserId
                         {where}
                         ORDER BY al.CreatedAt DESC";
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync(sql, new { Limit = limit, From = from, To = to, UserId = userId });
        }

        public async Task InsertAsync(int userId, string action, string tableName, int? recordId,
            string? oldValue = null, string? newValue = null, string? ipAddress = null)
        {
            var sql = @"INSERT INTO AuditLogs (UserId, Action, TableName, RecordId, OldValue, NewValue, IPAddress)
                        VALUES (@UserId, @Action, @TableName, @RecordId, @OldValue, @NewValue, @IPAddress)";
            await ExecuteSqlAsync(sql, new { UserId = userId, Action = action, TableName = tableName,
                RecordId = recordId, OldValue = oldValue, NewValue = newValue, IPAddress = ipAddress });
        }
    }
}
