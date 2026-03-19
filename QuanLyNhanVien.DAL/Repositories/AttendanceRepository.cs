using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class AttendanceRepository : BaseRepository
    {
        public AttendanceRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        /// <summary>
        /// Lấy danh sách chấm công theo ngày, có thể filter theo phòng ban
        /// </summary>
        public virtual async Task<IEnumerable<AttendanceRecord>> GetByDateAsync(DateTime workDate, int? departmentId = null)
        {
            var where = "WHERE a.WorkDate = @WorkDate";
            if (departmentId.HasValue)
                where += " AND e.DepartmentId = @DepartmentId";

            var sql = $@"SELECT a.*, e.FullName AS EmployeeName, e.EmployeeCode, d.DepartmentName
                         FROM AttendanceRecords a
                         INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
                         LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                         {where}
                         ORDER BY e.EmployeeCode";
            return await QuerySqlAsync<AttendanceRecord>(sql, new { WorkDate = workDate, DepartmentId = departmentId });
        }

        /// <summary>
        /// Lấy bảng chấm công tháng — trả về tất cả records trong tháng
        /// </summary>
        public virtual async Task<IEnumerable<AttendanceRecord>> GetMonthlyAsync(int month, int year, int? departmentId = null)
        {
            var where = "WHERE MONTH(a.WorkDate) = @Month AND YEAR(a.WorkDate) = @Year";
            if (departmentId.HasValue)
                where += " AND e.DepartmentId = @DepartmentId";

            var sql = $@"SELECT a.*, e.FullName AS EmployeeName, e.EmployeeCode, d.DepartmentName
                         FROM AttendanceRecords a
                         INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
                         LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                         {where}
                         ORDER BY e.EmployeeCode, a.WorkDate";
            return await QuerySqlAsync<AttendanceRecord>(sql, new { Month = month, Year = year, DepartmentId = departmentId });
        }

        /// <summary>
        /// Kiểm tra đã chấm công cho NV trong ngày chưa
        /// </summary>
        public virtual async Task<AttendanceRecord?> GetByEmployeeAndDateAsync(int employeeId, DateTime workDate)
        {
            var sql = @"SELECT a.*, e.FullName AS EmployeeName, e.EmployeeCode
                        FROM AttendanceRecords a
                        INNER JOIN Employees e ON a.EmployeeId = e.EmployeeId
                        WHERE a.EmployeeId = @EmployeeId AND a.WorkDate = @WorkDate";
            var result = await QuerySqlAsync<AttendanceRecord>(sql, new { EmployeeId = employeeId, WorkDate = workDate });
            return result.FirstOrDefault();
        }

        public virtual async Task<long> InsertAsync(AttendanceRecord record)
        {
            var sql = @"INSERT INTO AttendanceRecords (EmployeeId, WorkDate, CheckIn, CheckOut, ShiftType, [Status], OvertimeHours, Notes, CreatedBy)
                        VALUES (@EmployeeId, @WorkDate, @CheckIn, @CheckOut, @ShiftType, @Status, @OvertimeHours, @Notes, @CreatedBy);
                        SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<long>(sql, record);
        }

        public virtual async Task UpdateAsync(AttendanceRecord record)
        {
            var sql = @"UPDATE AttendanceRecords SET 
                        CheckIn = @CheckIn, CheckOut = @CheckOut, ShiftType = @ShiftType,
                        [Status] = @Status, OvertimeHours = @OvertimeHours, Notes = @Notes
                        WHERE RecordId = @RecordId";
            await ExecuteSqlAsync(sql, record);
        }

        public virtual async Task DeleteAsync(long recordId)
        {
            var sql = "DELETE FROM AttendanceRecords WHERE RecordId = @RecordId";
            await ExecuteSqlAsync(sql, new { RecordId = recordId });
        }

        /// <summary>
        /// Chấm công hàng loạt — insert nhiều records cùng lúc
        /// </summary>
        public virtual async Task<int> BulkInsertAsync(List<AttendanceRecord> records)
        {
            var sql = @"INSERT INTO AttendanceRecords (EmployeeId, WorkDate, CheckIn, CheckOut, ShiftType, [Status], OvertimeHours, Notes, CreatedBy)
                        VALUES (@EmployeeId, @WorkDate, @CheckIn, @CheckOut, @ShiftType, @Status, @OvertimeHours, @Notes, @CreatedBy)";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, records);
        }

        /// <summary>
        /// Lấy danh sách NV chưa chấm công ngày hôm đó (dùng cho bulk)
        /// </summary>
        public virtual async Task<IEnumerable<Models.Entities.Employee>> GetEmployeesNotCheckedAsync(DateTime workDate, int? departmentId = null)
        {
            var where = "WHERE e.IsActive = 1 AND e.EmployeeId NOT IN (SELECT EmployeeId FROM AttendanceRecords WHERE WorkDate = @WorkDate)";
            if (departmentId.HasValue)
                where += " AND e.DepartmentId = @DepartmentId";

            var sql = $@"SELECT e.EmployeeId, e.EmployeeCode, e.FullName, e.DepartmentId, d.DepartmentName
                         FROM Employees e
                         LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                         {where}
                         ORDER BY e.EmployeeCode";
            return await QuerySqlAsync<Models.Entities.Employee>(sql, new { WorkDate = workDate, DepartmentId = departmentId });
        }
    }
}
