using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class DepartmentRepository : BaseRepository
    {
        public DepartmentRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        public virtual async Task<IEnumerable<Department>> GetAllAsync(bool activeOnly = true)
        {
            var where = activeOnly ? "WHERE d.IsActive = 1" : "";
            var sql = $@"SELECT d.*, 
                        e.FullName AS ManagerName,
                        p.DepartmentName AS ParentName,
                        (SELECT COUNT(*) FROM Employees emp WHERE emp.DepartmentId = d.DepartmentId AND emp.IsActive = 1) AS EmployeeCount
                        FROM Departments d
                        LEFT JOIN Employees e ON d.ManagerId = e.EmployeeId
                        LEFT JOIN Departments p ON d.ParentId = p.DepartmentId
                        {where}
                        ORDER BY d.DepartmentCode";
            return await QuerySqlAsync<Department>(sql);
        }

        public virtual async Task<Department?> GetByIdAsync(int departmentId)
        {
            return (await QuerySqlAsync<Department>(
                "SELECT * FROM Departments WHERE DepartmentId = @Id", new { Id = departmentId })).FirstOrDefault();
        }

        public virtual async Task<int> InsertAsync(Department dept)
        {
            var sql = @"INSERT INTO Departments (DepartmentCode, DepartmentName, ParentId, ManagerId, IsActive)
                        VALUES (@DepartmentCode, @DepartmentName, @ParentId, @ManagerId, 1);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, dept);
        }

        public virtual async Task UpdateAsync(Department dept)
        {
            var sql = @"UPDATE Departments SET DepartmentCode = @DepartmentCode, DepartmentName = @DepartmentName,
                        ParentId = @ParentId, ManagerId = @ManagerId, IsActive = @IsActive
                        WHERE DepartmentId = @DepartmentId";
            await ExecuteSqlAsync(sql, dept);
        }

        public virtual async Task DeleteAsync(int departmentId)
        {
            await ExecuteSqlAsync("UPDATE Departments SET IsActive = 0 WHERE DepartmentId = @DepartmentId",
                new { DepartmentId = departmentId });
        }

        public virtual async Task<bool> HasEmployeesAsync(int departmentId)
        {
            using var conn = _dbFactory.CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Employees WHERE DepartmentId = @Id AND IsActive = 1",
                new { Id = departmentId });
            return count > 0;
        }
    }
}
