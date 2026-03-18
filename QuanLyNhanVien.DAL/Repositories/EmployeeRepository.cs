using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.Entities;
using QuanLyNhanVien.Models.DTOs;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class EmployeeRepository : BaseRepository
    {
        public EmployeeRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        public async Task<PaginationResult<Employee>> GetAllAsync(
            int pageIndex = 1, int pageSize = 50,
            string? search = null, int? departmentId = null, bool? isActive = null)
        {
            var result = new PaginationResult<Employee> { PageIndex = pageIndex, PageSize = pageSize };

            var where = "WHERE 1=1";
            if (!string.IsNullOrEmpty(search))
                where += " AND (e.FullName LIKE @Search OR e.EmployeeCode LIKE @Search OR e.Phone LIKE @Search)";
            if (departmentId.HasValue)
                where += " AND e.DepartmentId = @DepartmentId";
            if (isActive.HasValue)
                where += " AND e.IsActive = @IsActive";

            var countSql = $"SELECT COUNT(*) FROM Employees e {where}";
            var dataSql = $@"SELECT e.*, d.DepartmentName, p.PositionName
                            FROM Employees e
                            LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                            LEFT JOIN Positions p ON e.PositionId = p.PositionId
                            {where}
                            ORDER BY e.EmployeeCode
                            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                Search = $"%{search}%",
                DepartmentId = departmentId,
                IsActive = isActive,
                Offset = (pageIndex - 1) * pageSize,
                PageSize = pageSize
            };

            using var conn = _dbFactory.CreateConnection();
            result.TotalCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);
            result.Items = (await conn.QueryAsync<Employee>(dataSql, parameters)).ToList();

            return result;
        }

        public async Task<Employee?> GetByIdAsync(int employeeId)
        {
            var sql = @"SELECT e.*, d.DepartmentName, p.PositionName
                        FROM Employees e
                        LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                        LEFT JOIN Positions p ON e.PositionId = p.PositionId
                        WHERE e.EmployeeId = @EmployeeId";
            var result = await QuerySqlAsync<Employee>(sql, new { EmployeeId = employeeId });
            return result.FirstOrDefault();
        }

        public async Task<int> InsertAsync(Employee emp)
        {
            var sql = @"INSERT INTO Employees (EmployeeCode, UserId, FullName, Gender, DateOfBirth, 
                        IdentityNo, Phone, Email, [Address], Photo, DepartmentId, PositionId, 
                        HireDate, BankAccount, BankName, TaxCode, InsuranceNo, 
                        BasicSalary, SalaryCoefficient, NumberOfDependents, IsActive, Notes)
                        VALUES (@EmployeeCode, @UserId, @FullName, @Gender, @DateOfBirth,
                        @IdentityNo, @Phone, @Email, @Address, @Photo, @DepartmentId, @PositionId,
                        @HireDate, @BankAccount, @BankName, @TaxCode, @InsuranceNo,
                        @BasicSalary, @SalaryCoefficient, @NumberOfDependents, @IsActive, @Notes);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, emp);
        }

        public async Task UpdateAsync(Employee emp)
        {
            var sql = @"UPDATE Employees SET 
                        FullName = @FullName, Gender = @Gender, DateOfBirth = @DateOfBirth,
                        IdentityNo = @IdentityNo, Phone = @Phone, Email = @Email, 
                        [Address] = @Address, Photo = @Photo, DepartmentId = @DepartmentId,
                        PositionId = @PositionId, HireDate = @HireDate, TerminationDate = @TerminationDate,
                        BankAccount = @BankAccount, BankName = @BankName, TaxCode = @TaxCode,
                        InsuranceNo = @InsuranceNo, BasicSalary = @BasicSalary, 
                        SalaryCoefficient = @SalaryCoefficient, NumberOfDependents = @NumberOfDependents,
                        IsActive = @IsActive, Notes = @Notes, UpdatedAt = GETDATE()
                        WHERE EmployeeId = @EmployeeId";
            await ExecuteSqlAsync(sql, emp);
        }

        public async Task DeleteAsync(int employeeId)
        {
            // Soft delete
            var sql = "UPDATE Employees SET IsActive = 0, TerminationDate = GETDATE(), UpdatedAt = GETDATE() WHERE EmployeeId = @EmployeeId";
            await ExecuteSqlAsync(sql, new { EmployeeId = employeeId });
        }

        public async Task<string> GenerateNextCodeAsync()
        {
            // Dùng transaction + lock hint để tránh race condition
            var sql = @"SELECT TOP 1 EmployeeCode FROM Employees WITH (UPDLOCK, HOLDLOCK)
                        WHERE EmployeeCode LIKE 'NV-%' 
                        ORDER BY EmployeeCode DESC";
            using var conn = _dbFactory.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                var lastCode = await conn.ExecuteScalarAsync<string>(sql, transaction: tx);

                string newCode;
                if (string.IsNullOrEmpty(lastCode))
                {
                    newCode = "NV-0001";
                }
                else
                {
                    var numPart = lastCode.Replace("NV-", "");
                    newCode = int.TryParse(numPart, out int num)
                        ? $"NV-{(num + 1):D4}"
                        : $"NV-{DateTime.Now:yyyyMMddHHmmss}";
                }

                tx.Commit();
                return newCode;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId)
        {
            var sql = @"SELECT e.*, d.DepartmentName, p.PositionName
                        FROM Employees e
                        LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                        LEFT JOIN Positions p ON e.PositionId = p.PositionId
                        WHERE e.DepartmentId = @DepartmentId AND e.IsActive = 1
                        ORDER BY e.FullName";
            return await QuerySqlAsync<Employee>(sql, new { DepartmentId = departmentId });
        }

        /// <summary>
        /// Cập nhật hàng loạt 1 trường cho nhiều nhân viên
        /// </summary>
        public async Task<int> BatchUpdateFieldAsync(List<int> employeeIds, string fieldName, object value)
        {
            // Whitelist fields cho phép batch update (chống SQL injection)
            var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "DepartmentId", "PositionId", "SalaryCoefficient",
                "BasicSalary", "NumberOfDependents", "IsActive"
            };

            if (!allowedFields.Contains(fieldName))
                throw new ArgumentException($"Trường '{fieldName}' không được phép cập nhật hàng loạt.");

            if (employeeIds == null || employeeIds.Count == 0)
                return 0;

            var sql = $@"UPDATE Employees SET [{fieldName}] = @Value, UpdatedAt = GETDATE()
                         WHERE EmployeeId IN @Ids";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Value = value, Ids = employeeIds });
        }
    }
}
