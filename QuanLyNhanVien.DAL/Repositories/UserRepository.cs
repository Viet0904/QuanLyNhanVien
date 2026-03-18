using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.Entities;
using QuanLyNhanVien.Models.DTOs;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class UserRepository : BaseRepository
    {
        public UserRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var sql = "SELECT * FROM Users WHERE Username = @Username";
            var users = await QuerySqlAsync<User>(sql, new { Username = username });
            return users.FirstOrDefault();
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            var sql = "SELECT * FROM Users WHERE UserId = @UserId";
            var users = await QuerySqlAsync<User>(sql, new { UserId = userId });
            return users.FirstOrDefault();
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var sql = "UPDATE Users SET LastLogin = GETDATE(), FailedLoginCount = 0, LockedUntil = NULL WHERE UserId = @UserId";
            await ExecuteSqlAsync(sql, new { UserId = userId });
        }

        public async Task IncrementFailedLoginAsync(int userId)
        {
            var sql = @"UPDATE Users SET FailedLoginCount = FailedLoginCount + 1,
                        LockedUntil = CASE WHEN FailedLoginCount + 1 >= 5 THEN DATEADD(MINUTE, 15, GETDATE()) ELSE LockedUntil END
                        WHERE UserId = @UserId";
            await ExecuteSqlAsync(sql, new { UserId = userId });
        }

        public async Task<int> InsertAsync(User user)
        {
            var sql = @"INSERT INTO Users (Username, PasswordHash, Salt, IsActive) 
                        VALUES (@Username, @PasswordHash, @Salt, @IsActive);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task UpdatePasswordAsync(int userId, string passwordHash)
        {
            var sql = "UPDATE Users SET PasswordHash = @PasswordHash, UpdatedAt = GETDATE() WHERE UserId = @UserId";
            await ExecuteSqlAsync(sql, new { UserId = userId, PasswordHash = passwordHash });
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(int userId)
        {
            var sql = @"SELECT r.* FROM Roles r 
                        INNER JOIN UserRoles ur ON r.RoleId = ur.RoleId 
                        WHERE ur.UserId = @UserId AND r.IsActive = 1";
            return await QuerySqlAsync<Role>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<MenuPermissionDto>> GetUserPermissionsAsync(int userId)
        {
            var sql = @"SELECT DISTINCT m.MenuId, m.MenuCode, m.MenuName, m.ParentId, 
                        m.FormName, m.IconName, m.SortOrder,
                        MAX(CAST(rp.CanView AS INT)) AS CanView,
                        MAX(CAST(rp.CanAdd AS INT)) AS CanAdd,
                        MAX(CAST(rp.CanEdit AS INT)) AS CanEdit,
                        MAX(CAST(rp.CanDelete AS INT)) AS CanDelete,
                        MAX(CAST(rp.CanExport AS INT)) AS CanExport,
                        MAX(CAST(rp.CanPrint AS INT)) AS CanPrint
                        FROM Menus m
                        INNER JOIN RolePermissions rp ON m.MenuId = rp.MenuId
                        INNER JOIN UserRoles ur ON rp.RoleId = ur.RoleId
                        WHERE ur.UserId = @UserId 
                        AND m.IsActive = 1 AND m.IsVisible = 1
                        GROUP BY m.MenuId, m.MenuCode, m.MenuName, m.ParentId, 
                                 m.FormName, m.IconName, m.SortOrder
                        ORDER BY m.SortOrder";
            return await QuerySqlAsync<MenuPermissionDto>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<dynamic>> GetAllUsersAsync()
        {
            var sql = @"SELECT u.UserId, u.Username, u.IsActive, u.FailedLoginCount, 
                        u.LockedUntil, u.LastLogin, u.CreatedAt,
                        ISNULL(STRING_AGG(r.RoleName, ', '), N'(Chưa gán)') AS RoleNames
                        FROM Users u
                        LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
                        LEFT JOIN Roles r ON ur.RoleId = r.RoleId AND r.IsActive = 1
                        GROUP BY u.UserId, u.Username, u.IsActive, u.FailedLoginCount, 
                                 u.LockedUntil, u.LastLogin, u.CreatedAt
                        ORDER BY u.Username";
            return await QuerySqlAsync<dynamic>(sql);
        }

        public async Task UpdateUserAsync(int userId, bool isActive)
        {
            var sql = "UPDATE Users SET IsActive = @IsActive, UpdatedAt = GETDATE() WHERE UserId = @UserId";
            await ExecuteSqlAsync(sql, new { UserId = userId, IsActive = isActive });
        }

        public async Task ResetPasswordAsync(int userId, string passwordHash)
        {
            var sql = @"UPDATE Users SET PasswordHash = @PasswordHash, FailedLoginCount = 0, 
                        LockedUntil = NULL, UpdatedAt = GETDATE() WHERE UserId = @UserId";
            await ExecuteSqlAsync(sql, new { UserId = userId, PasswordHash = passwordHash });
        }

        public async Task<IEnumerable<int>> GetUserRoleIdsAsync(int userId)
        {
            var sql = "SELECT RoleId FROM UserRoles WHERE UserId = @UserId";
            return await QuerySqlAsync<int>(sql, new { UserId = userId });
        }

        public async Task ReplaceUserRolesAsync(int userId, List<int> roleIds)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                await conn.ExecuteAsync("DELETE FROM UserRoles WHERE UserId = @UserId",
                    new { UserId = userId }, tx);

                foreach (var roleId in roleIds)
                {
                    await conn.ExecuteAsync(
                        "INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)",
                        new { UserId = userId, RoleId = roleId }, tx);
                }
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
