using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Constants;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Services
{
    /// <summary>
    /// Quản lý người dùng: CRUD + gán role
    /// </summary>
    public class UserService
    {
        private readonly UserRepository _userRepo;
        private readonly RoleRepository _roleRepo;

        public UserService(UserRepository userRepo, RoleRepository roleRepo)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
        }

        /// <summary>
        /// Lấy danh sách tất cả user (kèm tên role)
        /// </summary>
        public async Task<IEnumerable<dynamic>> GetAllAsync()
            => await _userRepo.GetAllUsersAsync();

        /// <summary>
        /// Lấy danh sách role đang hoạt động
        /// </summary>
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
            => await _roleRepo.GetAllAsync();

        /// <summary>
        /// Lấy danh sách roleId đã gán cho user
        /// </summary>
        public async Task<IEnumerable<int>> GetUserRoleIdsAsync(int userId)
            => await _userRepo.GetUserRoleIdsAsync(userId);

        /// <summary>
        /// Tạo user mới + gán roles
        /// </summary>
        public async Task<(bool Ok, string Msg)> CreateAsync(string username, string password, List<int> roleIds)
        {
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Tên đăng nhập không được để trống.");
            if (password.Length < AppConstants.MinPasswordLength)
                return (false, $"Mật khẩu phải có ít nhất {AppConstants.MinPasswordLength} ký tự.");

            var existing = await _userRepo.GetByUsernameAsync(username);
            if (existing != null)
                return (false, "Tên đăng nhập đã tồn tại.");

            var hash = BCrypt.Net.BCrypt.HashPassword(password, AppConstants.BcryptWorkFactor);
            var user = new User
            {
                Username = username,
                PasswordHash = hash,
                Salt = "",
                IsActive = true
            };

            var userId = await _userRepo.InsertAsync(user);

            if (roleIds.Count > 0)
                await _userRepo.ReplaceUserRolesAsync(userId, roleIds);

            return (true, $"Tạo tài khoản '{username}' thành công!");
        }

        /// <summary>
        /// Cập nhật user (trạng thái + roles)
        /// </summary>
        public async Task<(bool Ok, string Msg)> UpdateAsync(int userId, bool isActive, List<int> roleIds)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return (false, "Không tìm thấy tài khoản.");

            await _userRepo.UpdateUserAsync(userId, isActive);
            await _userRepo.ReplaceUserRolesAsync(userId, roleIds);

            return (true, "Cập nhật thành công!");
        }

        /// <summary>
        /// Reset mật khẩu về mặc định (123456)
        /// </summary>
        public async Task<(bool Ok, string Msg)> ResetPasswordAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return (false, "Không tìm thấy tài khoản.");

            var defaultPassword = "123456";
            var hash = BCrypt.Net.BCrypt.HashPassword(defaultPassword, AppConstants.BcryptWorkFactor);
            await _userRepo.ResetPasswordAsync(userId, hash);

            return (true, $"Đã reset mật khẩu về '{defaultPassword}'.");
        }

        /// <summary>
        /// Vô hiệu hóa tài khoản (soft delete)
        /// </summary>
        public async Task<(bool Ok, string Msg)> DeactivateAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return (false, "Không tìm thấy tài khoản.");
            if (user.Username.ToLower() == "admin")
                return (false, "Không thể vô hiệu hóa tài khoản Admin.");

            await _userRepo.UpdateUserAsync(userId, false);
            return (true, "Đã vô hiệu hóa tài khoản.");
        }
    }
}
