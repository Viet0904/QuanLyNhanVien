using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Constants;
using QuanLyNhanVien.Models.DTOs;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Services
{
    /// <summary>
    /// Dịch vụ xác thực (login, đổi mật khẩu, tạo tài khoản)
    /// </summary>
    public class AuthService
    {
        private readonly UserRepository _userRepo;

        public AuthService(UserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            var result = new LoginResult();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                result.Message = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return result;
            }

            var user = await _userRepo.GetByUsernameAsync(username);
            if (user == null)
            {
                // Thực hiện BCrypt verify giả để tránh timing attack
                BCrypt.Net.BCrypt.Verify(password, "$2a$12$LJ3m4ys3Lg2VBe/GFa7X3OFfPBSPRJGjontPzTdlMboJj/BzGK0f.");
                result.Message = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return result;
            }

            if (!user.IsActive)
            {
                result.Message = "Tài khoản đã bị vô hiệu hóa.";
                return result;
            }

            // Kiểm tra lock
            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.Now)
            {
                var remaining = (user.LockedUntil.Value - DateTime.Now).Minutes + 1;
                result.Message = $"Tài khoản bị khóa. Vui lòng thử lại sau {remaining} phút.";
                return result;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                await _userRepo.IncrementFailedLoginAsync(user.UserId);
                var attemptsLeft = AppConstants.MaxLoginAttempts - user.FailedLoginCount - 1;
                result.Message = attemptsLeft > 0
                    ? "Tên đăng nhập hoặc mật khẩu không đúng."
                    : "Tài khoản đã bị khóa do nhập sai mật khẩu quá nhiều lần.";
                return result;
            }

            // Login thành công
            await _userRepo.UpdateLastLoginAsync(user.UserId);
            
            var roles = await _userRepo.GetUserRolesAsync(user.UserId);
            var permissions = await _userRepo.GetUserPermissionsAsync(user.UserId);

            result.Success = true;
            result.Message = "Đăng nhập thành công!";
            result.User = user;
            result.Roles = roles.ToList();
            result.Permissions = permissions.ToList();

            return result;
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var (isValid, validationMsg) = AppConstants.ValidatePasswordStrength(newPassword);
            if (!isValid)
                return (false, validationMsg);

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return (false, "Không tìm thấy tài khoản.");

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                return (false, "Mật khẩu cũ không đúng.");

            var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword, AppConstants.BcryptWorkFactor);
            await _userRepo.UpdatePasswordAsync(userId, newHash);

            return (true, "Đổi mật khẩu thành công!");
        }

        /// <summary>
        /// Tạo tài khoản mới
        /// </summary>
        public async Task<(bool Success, string Message, int UserId)> CreateUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "Vui lòng nhập đầy đủ thông tin.", 0);

            var existing = await _userRepo.GetByUsernameAsync(username);
            if (existing != null)
                return (false, "Tên đăng nhập đã tồn tại.", 0);

            var hash = BCrypt.Net.BCrypt.HashPassword(password, AppConstants.BcryptWorkFactor);
            var user = new User
            {
                Username = username,
                PasswordHash = hash,
                Salt = "",
                IsActive = true
            };

            var userId = await _userRepo.InsertAsync(user);
            return (true, "Tạo tài khoản thành công!", userId);
        }
    }
}
