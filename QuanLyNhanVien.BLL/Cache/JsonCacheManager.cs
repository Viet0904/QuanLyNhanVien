using Newtonsoft.Json;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Constants;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Cache
{
    /// <summary>
    /// Quản lý JSON cache cho danh mục
    /// Load từ DB → lưu JSON file → lần sau đọc từ JSON
    /// Invalidate khi user thay đổi danh mục
    /// </summary>
    public class JsonCacheManager
    {
        private readonly CategoryRepository _categoryRepo;
        private readonly string _cacheFilePath;
        private CacheData? _memoryCache;

        public JsonCacheManager(CategoryRepository categoryRepo, string basePath)
        {
            _categoryRepo = categoryRepo;
            _cacheFilePath = Path.Combine(basePath, AppConstants.CategoryCacheFile);
        }

        /// <summary>
        /// Lấy tất cả danh mục (từ cache hoặc DB)
        /// </summary>
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && _memoryCache != null && !IsCacheExpired(_memoryCache))
            {
                return _memoryCache.Categories;
            }

            // Thử đọc từ file JSON
            if (!forceRefresh && File.Exists(_cacheFilePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(_cacheFilePath);
                    var cacheData = JsonConvert.DeserializeObject<CacheData>(json);
                    if (cacheData != null && !IsCacheExpired(cacheData))
                    {
                        _memoryCache = cacheData;
                        return cacheData.Categories;
                    }
                }
                catch
                {
                    // JSON bị hỏng, load lại từ DB
                }
            }

            // Load từ DB và cache lại
            return await RefreshCacheAsync();
        }

        /// <summary>
        /// Lấy items theo mã danh mục
        /// </summary>
        public async Task<IEnumerable<CategoryItem>> GetItemsByCategoryCodeAsync(string categoryCode)
        {
            var categories = await GetAllCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.CategoryCode == categoryCode);
            return category?.Items ?? Enumerable.Empty<CategoryItem>();
        }

        /// <summary>
        /// Invalidate cache (gọi khi user thay đổi danh mục)
        /// </summary>
        public async Task<IEnumerable<Category>> InvalidateAndRefreshAsync()
        {
            _memoryCache = null;
            return await RefreshCacheAsync();
        }

        private async Task<IEnumerable<Category>> RefreshCacheAsync()
        {
            var categories = (await _categoryRepo.GetAllWithItemsAsync()).ToList();

            var cacheData = new CacheData
            {
                LastUpdated = DateTime.Now,
                CacheExpiryMinutes = AppConstants.CacheExpiryMinutes,
                Categories = categories
            };

            // Lưu ra file JSON
            try
            {
                var dir = Path.GetDirectoryName(_cacheFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(cacheData, Formatting.Indented);
                await File.WriteAllTextAsync(_cacheFilePath, json);
            }
            catch
            {
                // Không lưu được file cũng không sao, dùng memory cache
            }

            _memoryCache = cacheData;
            return categories;
        }

        private bool IsCacheExpired(CacheData cacheData)
        {
            return (DateTime.Now - cacheData.LastUpdated).TotalMinutes > cacheData.CacheExpiryMinutes;
        }

        private class CacheData
        {
            public DateTime LastUpdated { get; set; }
            public int CacheExpiryMinutes { get; set; }
            public List<Category> Categories { get; set; } = new();
        }
    }
}
