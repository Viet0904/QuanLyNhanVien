using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.DTOs;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Services
{
    /// <summary>
    /// Quản lý menu & phân quyền
    /// </summary>
    public class MenuService
    {
        private readonly MenuRepository _menuRepo;
        private readonly RoleRepository _roleRepo;

        public MenuService(MenuRepository menuRepo, RoleRepository roleRepo)
        {
            _menuRepo = menuRepo;
            _roleRepo = roleRepo;
        }

        /// <summary>
        /// Lấy menu tree đầy đủ (cho admin quản lý)
        /// </summary>
        public async Task<List<MenuNode>> GetMenuTreeAsync()
        {
            var allMenus = (await _menuRepo.GetAllAsync()).ToList();
            return BuildTree(allMenus);
        }

        /// <summary>
        /// Xây dựng menu tree từ danh sách phẳng quyền user
        /// </summary>
        public List<MenuPermissionDto> BuildPermissionTree(List<MenuPermissionDto> flatPermissions)
        {
            var lookup = flatPermissions.ToDictionary(x => x.MenuId);
            var roots = new List<MenuPermissionDto>();

            foreach (var perm in flatPermissions)
            {
                if (perm.ParentId == null || !lookup.ContainsKey(perm.ParentId.Value))
                {
                    roots.Add(perm);
                }
                else
                {
                    lookup[perm.ParentId.Value].Children.Add(perm);
                }
            }

            // Đảm bảo parent luôn hiển thị nếu có child có quyền
            return roots.Where(r => r.CanView || r.Children.Any(c => c.CanView)).ToList();
        }

        // Roles
        public async Task<IEnumerable<Role>> GetAllRolesAsync() => await _roleRepo.GetAllAsync();
        public async Task<int> CreateRoleAsync(Role role) => await _roleRepo.InsertAsync(role);
        public async Task UpdateRoleAsync(Role role) => await _roleRepo.UpdateAsync(role);

        // Permissions
        public async Task<IEnumerable<RolePermission>> GetRolePermissionsAsync(int roleId)
            => await _roleRepo.GetPermissionsAsync(roleId);
        
        public async Task SaveRolePermissionsAsync(int roleId, List<RolePermission> permissions)
            => await _roleRepo.SavePermissionsAsync(roleId, permissions);

        // User-Role
        public async Task AssignRoleAsync(int userId, int roleId)
            => await _roleRepo.AssignUserRoleAsync(userId, roleId);
        public async Task RemoveRoleAsync(int userId, int roleId)
            => await _roleRepo.RemoveUserRoleAsync(userId, roleId);

        // Menu CRUD
        public async Task<int> CreateMenuAsync(MenuNode menu) => await _menuRepo.InsertAsync(menu);
        public async Task UpdateMenuAsync(MenuNode menu) => await _menuRepo.UpdateAsync(menu);
        public async Task DeleteMenuAsync(int menuId) => await _menuRepo.DeleteAsync(menuId);

        private List<MenuNode> BuildTree(List<MenuNode> flatList)
        {
            var lookup = flatList.ToDictionary(x => x.MenuId);
            var roots = new List<MenuNode>();

            foreach (var node in flatList)
            {
                if (node.ParentId == null || !lookup.ContainsKey(node.ParentId.Value))
                {
                    roots.Add(node);
                }
                else
                {
                    lookup[node.ParentId.Value].Children.Add(node);
                }
            }

            return roots;
        }
    }
}
