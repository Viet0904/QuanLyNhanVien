using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class MenuRepository : BaseRepository
    {
        public MenuRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        public virtual async Task<IEnumerable<MenuNode>> GetAllAsync()
        {
            var sql = "SELECT * FROM Menus WHERE IsActive = 1 ORDER BY SortOrder";
            return await QuerySqlAsync<MenuNode>(sql);
        }

        public virtual async Task<int> InsertAsync(MenuNode menu)
        {
            var sql = @"INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder, IsVisible, IsActive, [Description])
                        VALUES (@MenuCode, @MenuName, @ParentId, @FormName, @IconName, @SortOrder, @IsVisible, @IsActive, @Description);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, menu);
        }

        public virtual async Task UpdateAsync(MenuNode menu)
        {
            var sql = @"UPDATE Menus SET MenuCode = @MenuCode, MenuName = @MenuName, ParentId = @ParentId,
                        FormName = @FormName, IconName = @IconName, SortOrder = @SortOrder,
                        IsVisible = @IsVisible, IsActive = @IsActive, [Description] = @Description
                        WHERE MenuId = @MenuId";
            await ExecuteSqlAsync(sql, menu);
        }

        public virtual async Task DeleteAsync(int menuId)
        {
            var sql = "UPDATE Menus SET IsActive = 0 WHERE MenuId = @MenuId";
            await ExecuteSqlAsync(sql, new { MenuId = menuId });
        }

        public virtual async Task UpdateSortOrderAsync(int menuId, int sortOrder)
        {
            var sql = "UPDATE Menus SET SortOrder = @SortOrder WHERE MenuId = @MenuId";
            await ExecuteSqlAsync(sql, new { MenuId = menuId, SortOrder = sortOrder });
        }
    }

    public class RoleRepository : BaseRepository
    {
        public RoleRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        public virtual async Task<IEnumerable<Role>> GetAllAsync()
        {
            var sql = "SELECT * FROM Roles WHERE IsActive = 1 ORDER BY RoleName";
            return await QuerySqlAsync<Role>(sql);
        }

        public virtual async Task<int> InsertAsync(Role role)
        {
            var sql = @"INSERT INTO Roles (RoleName, [Description], IsSystem, IsActive)
                        VALUES (@RoleName, @Description, @IsSystem, @IsActive);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, role);
        }

        public virtual async Task UpdateAsync(Role role)
        {
            var sql = @"UPDATE Roles SET RoleName = @RoleName, [Description] = @Description, IsActive = @IsActive
                        WHERE RoleId = @RoleId";
            await ExecuteSqlAsync(sql, role);
        }

        public virtual async Task<IEnumerable<RolePermission>> GetPermissionsAsync(int roleId)
        {
            var sql = @"SELECT rp.*, m.MenuName, m.MenuCode
                        FROM RolePermissions rp
                        INNER JOIN Menus m ON rp.MenuId = m.MenuId
                        WHERE rp.RoleId = @RoleId
                        ORDER BY m.SortOrder";
            return await QuerySqlAsync<RolePermission>(sql, new { RoleId = roleId });
        }

        public virtual async Task SavePermissionsAsync(int roleId, List<RolePermission> permissions)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();
            try
            {
                // Xóa quyền cũ
                await conn.ExecuteAsync("DELETE FROM RolePermissions WHERE RoleId = @RoleId",
                    new { RoleId = roleId }, transaction);

                // Insert quyền mới
                foreach (var perm in permissions)
                {
                    perm.RoleId = roleId;
                    await conn.ExecuteAsync(@"INSERT INTO RolePermissions (RoleId, MenuId, CanView, CanAdd, CanEdit, CanDelete, CanExport, CanPrint)
                        VALUES (@RoleId, @MenuId, @CanView, @CanAdd, @CanEdit, @CanDelete, @CanExport, @CanPrint)",
                        perm, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public virtual async Task AssignUserRoleAsync(int userId, int roleId)
        {
            var sql = @"IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
                        INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)";
            await ExecuteSqlAsync(sql, new { UserId = userId, RoleId = roleId });
        }

        public virtual async Task RemoveUserRoleAsync(int userId, int roleId)
        {
            var sql = "DELETE FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId";
            await ExecuteSqlAsync(sql, new { UserId = userId, RoleId = roleId });
        }
    }
}
