using Dapper;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.DAL.Repositories
{
    public class CompanySettingsRepository : BaseRepository
    {
        public CompanySettingsRepository(DbConnectionFactory dbFactory) : base(dbFactory) { }

        public async Task<IEnumerable<CompanySetting>> GetAllAsync()
        {
            var sql = "SELECT * FROM CompanySettings ORDER BY SettingKey";
            return await QuerySqlAsync<CompanySetting>(sql);
        }

        public async Task<string?> GetByKeyAsync(string key)
        {
            var sql = "SELECT SettingValue FROM CompanySettings WHERE SettingKey = @Key";
            var results = await QuerySqlAsync<string>(sql, new { Key = key });
            return results.FirstOrDefault();
        }

        public async Task UpsertAsync(string key, string value)
        {
            var sql = @"IF EXISTS (SELECT 1 FROM CompanySettings WHERE SettingKey = @Key)
                        UPDATE CompanySettings SET SettingValue = @Value, UpdatedAt = GETDATE() WHERE SettingKey = @Key
                        ELSE
                        INSERT INTO CompanySettings (SettingKey, SettingValue) VALUES (@Key, @Value)";
            await ExecuteSqlAsync(sql, new { Key = key, Value = value });
        }

        /// <summary>
        /// Lưu hàng loạt settings
        /// </summary>
        public async Task SaveAllAsync(Dictionary<string, string> settings)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                foreach (var (key, value) in settings)
                {
                    await conn.ExecuteAsync(
                        @"IF EXISTS (SELECT 1 FROM CompanySettings WHERE SettingKey = @Key)
                          UPDATE CompanySettings SET SettingValue = @Value, UpdatedAt = GETDATE() WHERE SettingKey = @Key
                          ELSE
                          INSERT INTO CompanySettings (SettingKey, SettingValue) VALUES (@Key, @Value)",
                        new { Key = key, Value = value }, tx);
                }
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
