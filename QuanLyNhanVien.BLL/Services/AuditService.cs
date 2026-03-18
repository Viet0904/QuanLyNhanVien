using QuanLyNhanVien.DAL.Repositories;

namespace QuanLyNhanVien.BLL.Services
{
    /// <summary>
    /// Service ghi audit log cho các hành động nhạy cảm.
    /// Tất cả phương thức đều fire-and-forget (không block caller).
    /// </summary>
    public class AuditService
    {
        private readonly AuditLogRepository _repo;

        public AuditService(AuditLogRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Ghi log hành động (fire-and-forget, không throw exception)
        /// </summary>
        public async Task LogAsync(int userId, string action, string tableName, int? recordId = null,
            string? oldValue = null, string? newValue = null)
        {
            try
            {
                await _repo.InsertAsync(userId, action, tableName, recordId, oldValue, newValue);
            }
            catch
            {
                // Audit log không nên làm crash app — swallow lỗi
            }
        }

        /// <summary>
        /// Ghi log hành động hệ thống (backup, restore, v.v.)
        /// </summary>
        public Task LogSystemAsync(int userId, string action, string? details = null)
            => LogAsync(userId, action, "System", null, null, details);

        /// <summary>
        /// Ghi log thay đổi user/quyền
        /// </summary>
        public Task LogUserChangeAsync(int performedByUserId, string action, int targetUserId, string? details = null)
            => LogAsync(performedByUserId, action, "Users", targetUserId, null, details);

        /// <summary>
        /// Ghi log thay đổi lương
        /// </summary>
        public Task LogSalaryAsync(int userId, string action, int? recordId = null, string? details = null)
            => LogAsync(userId, action, "SalaryRecords", recordId, null, details);
    }
}
