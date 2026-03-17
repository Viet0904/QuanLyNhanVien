namespace QuanLyNhanVien.Models.Entities
{
    public class MenuNode
    {
        public int MenuId { get; set; }
        public string MenuCode { get; set; } = string.Empty;
        public string MenuName { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? FormName { get; set; }
        public string? IconName { get; set; }
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }

        // Navigation
        public List<MenuNode> Children { get; set; } = new();
    }
}
