using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.DTOs;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class LeaveRequestRepository : BaseRepository
    {
        public LeaveRequestRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        /// <summary>
        /// Danh sách đơn phép — phân trang, filter theo NV và trạng thái
        /// </summary>
        public async Task<PaginationResult<LeaveRequest>> GetAllAsync(
            int? employeeId = null, string? status = null,
            int pageIndex = 1, int pageSize = 50)
        {
            var result = new PaginationResult<LeaveRequest> { PageIndex = pageIndex, PageSize = pageSize };

            var where = "WHERE 1=1";
            if (employeeId.HasValue)
                where += " AND lr.EmployeeId = @EmployeeId";
            if (!string.IsNullOrEmpty(status))
                where += " AND lr.[Status] = @Status";

            var countSql = $"SELECT COUNT(*) FROM LeaveRequests lr {where}";
            var dataSql = $@"SELECT lr.*, e.FullName AS EmployeeName, u.Username AS ApprovedByName
                             FROM LeaveRequests lr
                             INNER JOIN Employees e ON lr.EmployeeId = e.EmployeeId
                             LEFT JOIN Users u ON lr.ApprovedBy = u.UserId
                             {where}
                             ORDER BY lr.CreatedAt DESC
                             OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                EmployeeId = employeeId,
                Status = status,
                Offset = (pageIndex - 1) * pageSize,
                PageSize = pageSize
            };

            using var conn = _dbFactory.CreateConnection();
            result.TotalCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);
            result.Items = (await conn.QueryAsync<LeaveRequest>(dataSql, parameters)).ToList();

            return result;
        }

        public async Task<LeaveRequest?> GetByIdAsync(int leaveId)
        {
            var sql = @"SELECT lr.*, e.FullName AS EmployeeName, u.Username AS ApprovedByName
                        FROM LeaveRequests lr
                        INNER JOIN Employees e ON lr.EmployeeId = e.EmployeeId
                        LEFT JOIN Users u ON lr.ApprovedBy = u.UserId
                        WHERE lr.LeaveId = @LeaveId";
            var result = await QuerySqlAsync<LeaveRequest>(sql, new { LeaveId = leaveId });
            return result.FirstOrDefault();
        }

        public async Task<int> InsertAsync(LeaveRequest req)
        {
            var sql = @"INSERT INTO LeaveRequests (EmployeeId, LeaveType, StartDate, EndDate, TotalDays, Reason, [Status])
                        VALUES (@EmployeeId, @LeaveType, @StartDate, @EndDate, @TotalDays, @Reason, @Status);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, req);
        }

        /// <summary>
        /// Duyệt hoặc từ chối đơn phép
        /// </summary>
        public async Task UpdateStatusAsync(int leaveId, string status, int approvedBy)
        {
            var sql = @"UPDATE LeaveRequests SET [Status] = @Status, ApprovedBy = @ApprovedBy, ApprovedAt = GETDATE()
                        WHERE LeaveId = @LeaveId";
            await ExecuteSqlAsync(sql, new { LeaveId = leaveId, Status = status, ApprovedBy = approvedBy });
        }

        public async Task DeleteAsync(int leaveId)
        {
            var sql = "DELETE FROM LeaveRequests WHERE LeaveId = @LeaveId AND [Status] = 'Pending'";
            await ExecuteSqlAsync(sql, new { LeaveId = leaveId });
        }

        /// <summary>
        /// Tính tổng số ngày phép đã dùng trong năm
        /// </summary>
        public async Task<decimal> GetUsedLeaveDaysAsync(int employeeId, int year)
        {
            var sql = @"SELECT ISNULL(SUM(TotalDays), 0) FROM LeaveRequests 
                        WHERE EmployeeId = @EmployeeId AND YEAR(StartDate) = @Year AND [Status] = 'Approved'";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<decimal>(sql, new { EmployeeId = employeeId, Year = year });
        }

        /// <summary>
        /// Kiểm tra trùng khoảng ngày nghỉ
        /// </summary>
        public async Task<bool> HasOverlappingLeaveAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludeLeaveId = null)
        {
            var sql = @"SELECT COUNT(*) FROM LeaveRequests 
                        WHERE EmployeeId = @EmployeeId 
                        AND [Status] != 'Rejected'
                        AND StartDate <= @EndDate AND EndDate >= @StartDate";
            if (excludeLeaveId.HasValue)
                sql += " AND LeaveId != @ExcludeLeaveId";

            using var conn = _dbFactory.CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId, StartDate = startDate, EndDate = endDate, ExcludeLeaveId = excludeLeaveId });
            return count > 0;
        }
    }
}
