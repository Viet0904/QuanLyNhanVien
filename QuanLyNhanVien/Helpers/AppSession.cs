using QuanLyNhanVien.Models.DTOs;
using QuanLyNhanVien.Models.Entities;
using QuanLyNhanVien.Models.Enums;

namespace QuanLyNhanVien.Helpers
{
    /// <summary>
    /// Lưu thông tin phiên đăng nhập hiện tại (static, sống suốt app lifetime)
    /// </summary>
    public static class AppSession
    {
        public static User? CurrentUser { get; set; }
        public static List<MenuPermissionDto> Permissions { get; set; } = new();
        public static List<Role> Roles { get; set; } = new();

        /// <summary>
        /// Kiểm tra user có quyền trên menu không
        /// </summary>
        public static bool HasPermission(string menuCode, PermissionType permType)
        {
            // Admin luôn có quyền
            if (Roles.Any(r => r.RoleName == "Administrator"))
                return true;

            var perm = Permissions.FirstOrDefault(p => p.MenuCode == menuCode);
            if (perm == null) return false;

            return permType switch
            {
                PermissionType.View => perm.CanView,
                PermissionType.Add => perm.CanAdd,
                PermissionType.Edit => perm.CanEdit,
                PermissionType.Delete => perm.CanDelete,
                PermissionType.Export => perm.CanExport,
                PermissionType.Print => perm.CanPrint,
                _ => false
            };
        }

        /// <summary>
        /// Lấy thông tin quyền cho một menu cụ thể
        /// </summary>
        public static MenuPermissionDto? GetMenuPermission(string menuCode)
        {
            return Permissions.FirstOrDefault(p => p.MenuCode == menuCode);
        }

        /// <summary>
        /// Xóa session (logout)
        /// </summary>
        public static void Clear()
        {
            CurrentUser = null;
            Permissions.Clear();
            Roles.Clear();
        }

        public static bool IsLoggedIn => CurrentUser != null;
        public static string DisplayName => CurrentUser?.Username ?? "N/A";
        public static string RoleDisplay => string.Join(", ", Roles.Select(r => r.RoleName));
    }
}
