using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class CategoryRepository : BaseRepository
    {
        public CategoryRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        public virtual async Task<IEnumerable<Category>> GetAllWithItemsAsync()
        {
            var sql = @"SELECT * FROM Categories ORDER BY CategoryCode;
                        SELECT * FROM CategoryItems WHERE IsActive = 1 ORDER BY CategoryId, SortOrder;";

            using var conn = _dbFactory.CreateConnection();
            conn.Open();
            using var multi = await conn.QueryMultipleAsync(sql);

            var categories = (await multi.ReadAsync<Category>()).ToList();
            var items = (await multi.ReadAsync<CategoryItem>()).ToList();

            foreach (var cat in categories)
            {
                cat.Items = items.Where(i => i.CategoryId == cat.CategoryId).ToList();
            }

            return categories;
        }

        public virtual async Task<IEnumerable<CategoryItem>> GetItemsByCategoryCodeAsync(string categoryCode)
        {
            var sql = @"SELECT ci.* FROM CategoryItems ci
                        INNER JOIN Categories c ON ci.CategoryId = c.CategoryId
                        WHERE c.CategoryCode = @CategoryCode AND ci.IsActive = 1
                        ORDER BY ci.SortOrder";
            return await QuerySqlAsync<CategoryItem>(sql, new { CategoryCode = categoryCode });
        }

        public virtual async Task<int> InsertCategoryAsync(Category cat)
        {
            var sql = @"INSERT INTO Categories (CategoryCode, CategoryName, [Description], IsSystem)
                        VALUES (@CategoryCode, @CategoryName, @Description, @IsSystem);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, cat);
        }

        public virtual async Task<int> InsertItemAsync(CategoryItem item)
        {
            var sql = @"INSERT INTO CategoryItems (CategoryId, ItemCode, ItemName, ItemValue, SortOrder, IsActive)
                        VALUES (@CategoryId, @ItemCode, @ItemName, @ItemValue, @SortOrder, @IsActive);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var conn = _dbFactory.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, item);
        }

        public virtual async Task UpdateItemAsync(CategoryItem item)
        {
            var sql = @"UPDATE CategoryItems SET ItemCode = @ItemCode, ItemName = @ItemName,
                        ItemValue = @ItemValue, SortOrder = @SortOrder, IsActive = @IsActive
                        WHERE ItemId = @ItemId";
            await ExecuteSqlAsync(sql, item);
        }
        public virtual async Task UpdateCategoryAsync(Category cat)
        {
            var sql = @"UPDATE Categories SET CategoryCode = @CategoryCode, CategoryName = @CategoryName,
                        [Description] = @Description WHERE CategoryId = @CategoryId";
            await ExecuteSqlAsync(sql, cat);
        }

        public virtual async Task DeleteItemAsync(int itemId)
        {
            await ExecuteSqlAsync("UPDATE CategoryItems SET IsActive = 0 WHERE ItemId = @ItemId", new { ItemId = itemId });
        }

        public virtual async Task DeleteCategoryAsync(int categoryId)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            await conn.ExecuteAsync("DELETE FROM CategoryItems WHERE CategoryId = @Id", new { Id = categoryId }, tx);
            await conn.ExecuteAsync("DELETE FROM Categories WHERE CategoryId = @Id AND IsSystem = 0", new { Id = categoryId }, tx);
            tx.Commit();
        }
    }
}
