using Moq;
using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.DTOs;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Tests
{
    /// <summary>
    /// Unit tests cho AuthService — Đăng nhập, đổi mật khẩu, tạo user
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<UserRepository> _mockUserRepo;
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _mockUserRepo = new Mock<UserRepository>(MockBehavior.Loose, new object[] { null! });
            _sut = new AuthService(_mockUserRepo.Object);
        }

        // ===== LoginAsync Tests =====

        [Fact]
        public async Task LoginAsync_EmptyUsername_Fails()
        {
            var result = await _sut.LoginAsync("", "pass123");
            Assert.False(result.Success);
            Assert.Contains("nhập tên đăng nhập", result.Message);
        }

        [Fact]
        public async Task LoginAsync_EmptyPassword_Fails()
        {
            var result = await _sut.LoginAsync("admin", "");
            Assert.False(result.Success);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_Fails()
        {
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("unknown"))
                .ReturnsAsync((User?)null);

            var result = await _sut.LoginAsync("unknown", "pass123");
            Assert.False(result.Success);
            Assert.Contains("không đúng", result.Message);
        }

        [Fact]
        public async Task LoginAsync_InactiveUser_Fails()
        {
            var user = new User
            {
                UserId = 1,
                Username = "inactive_user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123"),
                IsActive = false
            };
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("inactive_user")).ReturnsAsync(user);

            var result = await _sut.LoginAsync("inactive_user", "Test123");
            Assert.False(result.Success);
            Assert.Contains("vô hiệu hóa", result.Message);
        }

        [Fact]
        public async Task LoginAsync_LockedUser_Fails()
        {
            var user = new User
            {
                UserId = 1,
                Username = "locked_user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123"),
                IsActive = true,
                LockedUntil = DateTime.Now.AddMinutes(10)
            };
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("locked_user")).ReturnsAsync(user);

            var result = await _sut.LoginAsync("locked_user", "Test123");
            Assert.False(result.Success);
            Assert.Contains("khóa", result.Message);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_IncrementsFailed()
        {
            var user = new User
            {
                UserId = 1,
                Username = "test_user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1"),
                IsActive = true,
                FailedLoginCount = 0
            };
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("test_user")).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.IncrementFailedLoginAsync(1)).Returns(Task.CompletedTask);

            var result = await _sut.LoginAsync("test_user", "WrongPass1");
            Assert.False(result.Success);
            _mockUserRepo.Verify(r => r.IncrementFailedLoginAsync(1), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_CorrectPassword_Succeeds()
        {
            var password = "Test@123";
            var user = new User
            {
                UserId = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = true
            };
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.UpdateLastLoginAsync(1)).Returns(Task.CompletedTask);
            _mockUserRepo.Setup(r => r.GetUserRolesAsync(1))
                .ReturnsAsync(new List<Role> { new() { RoleId = 1, RoleName = "Admin" } });
            _mockUserRepo.Setup(r => r.GetUserPermissionsAsync(1))
                .ReturnsAsync(new List<MenuPermissionDto>());

            var result = await _sut.LoginAsync("admin", password);
            Assert.True(result.Success);
            Assert.NotNull(result.User);
            Assert.Single(result.Roles);
            _mockUserRepo.Verify(r => r.UpdateLastLoginAsync(1), Times.Once);
        }

        // ===== ChangePasswordAsync Tests =====

        [Fact]
        public async Task ChangePasswordAsync_WeakPassword_Fails()
        {
            // password "123" quá yếu (< 6 chars, không có chữ hoa/thường)
            var result = await _sut.ChangePasswordAsync(1, "OldPass1", "123");
            Assert.False(result.Success);
        }

        [Fact]
        public async Task ChangePasswordAsync_UserNotFound_Fails()
        {
            _mockUserRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

            var result = await _sut.ChangePasswordAsync(999, "OldPass1", "NewPass1");
            Assert.False(result.Success);
            Assert.Contains("Không tìm thấy", result.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_WrongOldPassword_Fails()
        {
            var user = new User
            {
                UserId = 1,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectOld1")
            };
            _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            var result = await _sut.ChangePasswordAsync(1, "WrongOld1", "NewPass1");
            Assert.False(result.Success);
            Assert.Contains("Mật khẩu cũ", result.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_ValidChange_Succeeds()
        {
            var oldPassword = "OldPass1";
            var user = new User
            {
                UserId = 1,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(oldPassword)
            };
            _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.UpdatePasswordAsync(1, It.IsAny<string>())).Returns(Task.CompletedTask);

            var result = await _sut.ChangePasswordAsync(1, oldPassword, "NewPass1");
            Assert.True(result.Success);
        }

        // ===== CreateUserAsync Tests =====

        [Fact]
        public async Task CreateUserAsync_EmptyInput_Fails()
        {
            var result = await _sut.CreateUserAsync("", "");
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateUserAsync_DuplicateUsername_Fails()
        {
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("admin"))
                .ReturnsAsync(new User { UserId = 1 });

            var result = await _sut.CreateUserAsync("admin", "Pass123");
            Assert.False(result.Success);
            Assert.Contains("đã tồn tại", result.Message);
        }

        [Fact]
        public async Task CreateUserAsync_ValidInput_Succeeds()
        {
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("newuser"))
                .ReturnsAsync((User?)null);
            _mockUserRepo.Setup(r => r.InsertAsync(It.IsAny<User>()))
                .ReturnsAsync(42);

            var result = await _sut.CreateUserAsync("newuser", "Pass123");
            Assert.True(result.Success);
            Assert.Equal(42, result.UserId);
        }
    }
}
