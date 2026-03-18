using Dapper;
using QuanLyNhanVien.DAL.Context;
using System.Text.RegularExpressions;
using QuanLyNhanVien.BLL.Services;

namespace QuanLyNhanVien.BLL.Services
{
    /// <summary>
    /// Sao lưu & phục hồi database SQL Server
    /// </summary>
    public class BackupService
    {
        private readonly DbConnectionFactory _dbFactory;
        private readonly string _databaseName;
        private readonly AuditService _audit;

        // Regex chỉ cho phép ký tự hợp lệ trong tên database — chống SQL injection
        private static readonly Regex ValidDbNameRegex = new(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);

        public BackupService(DbConnectionFactory dbFactory, AuditService audit, string databaseName = "QuanLyNhanVien")
        {
            _dbFactory = dbFactory;
            _audit = audit;

            if (string.IsNullOrWhiteSpace(databaseName) || !ValidDbNameRegex.IsMatch(databaseName))
                throw new ArgumentException($"Tên database không hợp lệ: '{databaseName}'. Chỉ chấp nhận chữ, số và dấu gạch dưới.", nameof(databaseName));

            _databaseName = databaseName;
        }

        /// <summary>
        /// Sao lưu database ra file .bak
        /// </summary>
        public async Task<(bool Ok, string Msg)> BackupAsync(string backupFolder)
        {
            try
            {
                if (!Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);

                var fileName = $"{_databaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                var backupPath = Path.Combine(backupFolder, fileName);

                var sql = $"BACKUP DATABASE [{_databaseName}] TO DISK = @Path WITH FORMAT, INIT, NAME = @Desc";

                using var conn = _dbFactory.CreateConnection();
                conn.Open();
                // Backup có thể mất thời gian, tăng timeout
                await conn.ExecuteAsync(sql,
                    new { Path = backupPath, Desc = $"Backup {_databaseName} - {DateTime.Now:dd/MM/yyyy HH:mm}" },
                    commandTimeout: 300);

                var fileInfo = new FileInfo(backupPath);
                var sizeMB = fileInfo.Length / (1024.0 * 1024.0);

                return (true, $"Sao lưu thành công!\nFile: {backupPath}\nKích thước: {sizeMB:F2} MB");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi sao lưu: {ex.Message}");
            }
        }

        /// <summary>
        /// Phục hồi database từ file .bak
        /// </summary>
        public async Task<(bool Ok, string Msg)> RestoreAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return (false, "File backup không tồn tại.");

                // Cần chuyển sang master database để restore
                using var conn = _dbFactory.CreateConnection();
                conn.Open();

                // Kick all connections
                await conn.ExecuteAsync(
                    $"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                    commandTimeout: 60);

                // Restore
                await conn.ExecuteAsync(
                    $"RESTORE DATABASE [{_databaseName}] FROM DISK = @Path WITH REPLACE",
                    new { Path = backupPath },
                    commandTimeout: 600);

                // Set back to multi-user
                await conn.ExecuteAsync(
                    $"ALTER DATABASE [{_databaseName}] SET MULTI_USER",
                    commandTimeout: 60);

                return (true, "Phục hồi database thành công!\nVui lòng khởi động lại ứng dụng.");
            }
            catch (Exception ex)
            {
                // Cố gắng set lại multi-user nếu bị lỗi giữa chừng
                try
                {
                    using var conn = _dbFactory.CreateConnection();
                    conn.Open();
                    await conn.ExecuteAsync($"ALTER DATABASE [{_databaseName}] SET MULTI_USER");
                }
                catch { /* Ignore */ }

                return (false, $"Lỗi phục hồi: {ex.Message}");
            }
        }

        /// <summary>
        /// Liệt kê file backup trong folder
        /// </summary>
        public List<(string FileName, long SizeBytes, DateTime CreatedAt)> GetBackupFiles(string folder)
        {
            if (!Directory.Exists(folder)) return new();

            return Directory.GetFiles(folder, "*.bak")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .Select(f => (f.Name, f.Length, f.CreationTime))
                .ToList();
        }
    }
}
