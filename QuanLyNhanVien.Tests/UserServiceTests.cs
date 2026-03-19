using Moq;
using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Tests
{
    /// <summary>
    /// Unit tests cho UserService — Quản lý tài khoản
    /// </summary>
    public class UserServiceTests
    {
        private readonly Mock<UserRepository> _mockUserRepo;
        private readonly Mock<RoleRepository> _mockRoleRepo;
        private readonly Mock<AuditLogRepository> _mockAuditRepo;
        private readonly UserService _sut;

        public UserServiceTests()
        {
            _mockUserRepo = new Mock<UserRepository>(MockBehavior.Loose, new object[] { null! });
            _mockRoleRepo = new Mock<RoleRepository>(MockBehavior.Loose, new object[] { null! });
            _mockAuditRepo = new Mock<AuditLogRepository>(MockBehavior.Loose, new object[] { null! });

            _sut = new UserService(_mockUserRepo.Object, _mockRoleRepo.Object,
                new AuditService(_mockAuditRepo.Object));
        }

        // ===== CreateAsync Tests =====

        [Fact]
        public async Task CreateAsync_EmptyUsername_Fails()
        {
            var result = await _sut.CreateAsync("", "Pass123", new List<int> { 1 });
            Assert.False(result.Ok);
            Assert.Contains("đăng nhập", result.Msg);
        }

        [Fact]
        public async Task CreateAsync_DuplicateUsername_Fails()
        {
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("admin"))
                .ReturnsAsync(new User { UserId = 1 });

            var result = await _sut.CreateAsync("admin", "Pass123", new List<int> { 1 });
            Assert.False(result.Ok);
            Assert.Contains("đã tồn tại", result.Msg);
        }

        [Fact]
        public async Task CreateAsync_WeakPassword_Fails()
        {
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("newuser"))
                .ReturnsAsync((User?)null);

            var result = await _sut.CreateAsync("newuser", "123", new List<int> { 1 });
            Assert.False(result.Ok);
        }

        [Fact]
        public async Task CreateAsync_ValidInput_Succeeds()
        {
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("newuser"))
                .ReturnsAsync((User?)null);
            _mockUserRepo.Setup(r => r.InsertAsync(It.IsAny<User>()))
                .ReturnsAsync(10);
            _mockUserRepo.Setup(r => r.ReplaceUserRolesAsync(10, It.IsAny<List<int>>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.CreateAsync("newuser", "Pass123", new List<int> { 1, 2 });
            Assert.True(result.Ok);
        }

        // ===== ResetPasswordAsync Tests =====

        [Fact]
        public async Task ResetPasswordAsync_UserNotFound_Fails()
        {
            _mockUserRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

            var result = await _sut.ResetPasswordAsync(999);
            Assert.False(result.Ok);
            Assert.Contains("Không tìm thấy", result.Msg);
        }

        [Fact]
        public async Task ResetPasswordAsync_ValidUser_Succeeds()
        {
            _mockUserRepo.Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(new User { UserId = 5, Username = "user5" });
            _mockUserRepo.Setup(r => r.ResetPasswordAsync(5, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.ResetPasswordAsync(5);
            Assert.True(result.Ok);
            Assert.Contains("reset", result.Msg);
        }

        // ===== DeactivateAsync Tests =====

        [Fact]
        public async Task DeactivateAsync_UserNotFound_Fails()
        {
            _mockUserRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

            var result = await _sut.DeactivateAsync(999);
            Assert.False(result.Ok);
        }

        [Fact]
        public async Task DeactivateAsync_AdminUser_Fails()
        {
            _mockUserRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new User { UserId = 1, Username = "admin" });

            var result = await _sut.DeactivateAsync(1);
            Assert.False(result.Ok);
            Assert.Contains("Admin", result.Msg);
        }

        [Fact]
        public async Task DeactivateAsync_ValidUser_Succeeds()
        {
            _mockUserRepo.Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(new User { UserId = 5, Username = "user5", IsActive = true });
            _mockUserRepo.Setup(r => r.UpdateUserAsync(5, false)).Returns(Task.CompletedTask);

            var result = await _sut.DeactivateAsync(5);
            Assert.True(result.Ok);
        }
    }
}
