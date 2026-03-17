namespace QuanLyNhanVien.Models.DTOs
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Entities.User? User { get; set; }
        public List<MenuPermissionDto> Permissions { get; set; } = new();
        public List<Entities.Role> Roles { get; set; } = new();
    }

    public class MenuPermissionDto
    {
        public int MenuId { get; set; }
        public string MenuCode { get; set; } = string.Empty;
        public string MenuName { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? FormName { get; set; }
        public string? IconName { get; set; }
        public int SortOrder { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExport { get; set; }
        public bool CanPrint { get; set; }

        // Tree helpers
        public List<MenuPermissionDto> Children { get; set; } = new();
    }
}
