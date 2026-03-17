using QuanLyNhanVien.BLL.Cache;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _catRepo;
        private readonly JsonCacheManager _cache;

        public CategoryService(CategoryRepository catRepo, JsonCacheManager cache)
        {
            _catRepo = catRepo;
            _cache = cache;
        }

        public async Task<IEnumerable<Category>> GetAllAsync(bool forceRefresh = false)
            => await _cache.GetAllCategoriesAsync(forceRefresh);

        public async Task<IEnumerable<CategoryItem>> GetItemsAsync(string categoryCode)
            => await _cache.GetItemsByCategoryCodeAsync(categoryCode);

        public async Task<(bool Ok, string Msg, int Id)> CreateCategoryAsync(Category cat)
        {
            if (string.IsNullOrWhiteSpace(cat.CategoryCode))
                return (false, "Vui lòng nhập mã danh mục.", 0);
            if (string.IsNullOrWhiteSpace(cat.CategoryName))
                return (false, "Vui lòng nhập tên danh mục.", 0);

            var id = await _catRepo.InsertCategoryAsync(cat);
            await _cache.InvalidateAndRefreshAsync();
            return (true, $"Thêm danh mục '{cat.CategoryName}' thành công!", id);
        }

        public async Task<(bool Ok, string Msg)> UpdateCategoryAsync(Category cat)
        {
            if (string.IsNullOrWhiteSpace(cat.CategoryName))
                return (false, "Vui lòng nhập tên danh mục.");

            await _catRepo.UpdateCategoryAsync(cat);
            await _cache.InvalidateAndRefreshAsync();
            return (true, "Cập nhật danh mục thành công!");
        }

        public async Task<(bool Ok, string Msg)> DeleteCategoryAsync(int catId)
        {
            await _catRepo.DeleteCategoryAsync(catId);
            await _cache.InvalidateAndRefreshAsync();
            return (true, "Xóa danh mục thành công!");
        }

        public async Task<(bool Ok, string Msg, int Id)> CreateItemAsync(CategoryItem item)
        {
            if (string.IsNullOrWhiteSpace(item.ItemCode))
                return (false, "Vui lòng nhập mã mục.", 0);
            if (string.IsNullOrWhiteSpace(item.ItemName))
                return (false, "Vui lòng nhập tên mục.", 0);

            var id = await _catRepo.InsertItemAsync(item);
            await _cache.InvalidateAndRefreshAsync();
            return (true, "Thêm mục thành công!", id);
        }

        public async Task<(bool Ok, string Msg)> UpdateItemAsync(CategoryItem item)
        {
            await _catRepo.UpdateItemAsync(item);
            await _cache.InvalidateAndRefreshAsync();
            return (true, "Cập nhật mục thành công!");
        }

        public async Task<(bool Ok, string Msg)> DeleteItemAsync(int itemId)
        {
            await _catRepo.DeleteItemAsync(itemId);
            await _cache.InvalidateAndRefreshAsync();
            return (true, "Xóa mục thành công!");
        }
    }
}
