using System.Collections.Generic;

namespace QuanLyNhanVien.Models.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystem { get; set; }

        public List<CategoryItem> Items { get; set; } = new();
    }

    public class CategoryItem
    {
        public int ItemId { get; set; }
        public int CategoryId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string? ItemValue { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
