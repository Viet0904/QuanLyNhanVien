using Moq;
using QuanLyNhanVien.BLL.Cache;
using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Tests
{
    /// <summary>
    /// Unit tests cho CategoryService — Quản lý danh mục
    /// </summary>
    public class CategoryServiceTests
    {
        private readonly Mock<CategoryRepository> _mockCatRepo;
        private readonly Mock<JsonCacheManager> _mockCache;
        private readonly CategoryService _sut;

        public CategoryServiceTests()
        {
            _mockCatRepo = new Mock<CategoryRepository>(MockBehavior.Loose, new object[] { null! });
            _mockCache = new Mock<JsonCacheManager>(MockBehavior.Loose, new object[] { _mockCatRepo.Object, "." });

            _sut = new CategoryService(_mockCatRepo.Object, _mockCache.Object);
        }

        // ===== CreateCategoryAsync Tests =====

        [Fact]
        public async Task CreateCategoryAsync_EmptyCode_Fails()
        {
            var cat = new Category { CategoryCode = "", CategoryName = "Test" };
            var result = await _sut.CreateCategoryAsync(cat);
            Assert.False(result.Ok);
            Assert.Contains("mã danh mục", result.Msg);
        }

        [Fact]
        public async Task CreateCategoryAsync_EmptyName_Fails()
        {
            var cat = new Category { CategoryCode = "TEST", CategoryName = "" };
            var result = await _sut.CreateCategoryAsync(cat);
            Assert.False(result.Ok);
            Assert.Contains("tên danh mục", result.Msg);
        }

        [Fact]
        public async Task CreateCategoryAsync_Valid_SucceedsAndInvalidatesCache()
        {
            var cat = new Category { CategoryCode = "TEST", CategoryName = "Test Category" };
            _mockCatRepo.Setup(r => r.InsertCategoryAsync(cat)).ReturnsAsync(1);
            _mockCache.Setup(c => c.InvalidateAndRefreshAsync())
                .ReturnsAsync(new List<Category>());

            var result = await _sut.CreateCategoryAsync(cat);
            Assert.True(result.Ok);
            _mockCache.Verify(c => c.InvalidateAndRefreshAsync(), Times.Once);
        }

        // ===== CreateItemAsync Tests =====

        [Fact]
        public async Task CreateItemAsync_EmptyCode_Fails()
        {
            var item = new CategoryItem { CategoryId = 1, ItemCode = "", ItemName = "Item" };
            var result = await _sut.CreateItemAsync(item);
            Assert.False(result.Ok);
            Assert.Contains("mã", result.Msg);
        }

        [Fact]
        public async Task CreateItemAsync_EmptyName_Fails()
        {
            var item = new CategoryItem { CategoryId = 1, ItemCode = "ITM", ItemName = "" };
            var result = await _sut.CreateItemAsync(item);
            Assert.False(result.Ok);
            Assert.Contains("tên", result.Msg);
        }

        [Fact]
        public async Task CreateItemAsync_Valid_SucceedsAndInvalidatesCache()
        {
            var item = new CategoryItem { CategoryId = 1, ItemCode = "ITM01", ItemName = "Item 01" };
            _mockCatRepo.Setup(r => r.InsertItemAsync(item)).ReturnsAsync(10);
            _mockCache.Setup(c => c.InvalidateAndRefreshAsync())
                .ReturnsAsync(new List<Category>());

            var result = await _sut.CreateItemAsync(item);
            Assert.True(result.Ok);
            _mockCache.Verify(c => c.InvalidateAndRefreshAsync(), Times.Once);
        }

        // ===== DeleteCategoryAsync Tests =====

        [Fact]
        public async Task DeleteCategoryAsync_Success()
        {
            _mockCatRepo.Setup(r => r.DeleteCategoryAsync(1)).Returns(Task.CompletedTask);
            _mockCache.Setup(c => c.InvalidateAndRefreshAsync())
                .ReturnsAsync(new List<Category>());

            var result = await _sut.DeleteCategoryAsync(1);
            Assert.True(result.Ok);
            _mockCache.Verify(c => c.InvalidateAndRefreshAsync(), Times.Once);
        }
    }
}
