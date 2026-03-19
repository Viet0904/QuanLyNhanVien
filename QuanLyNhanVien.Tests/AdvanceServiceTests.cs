using Moq;
using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Tests
{
    /// <summary>
    /// Unit tests cho AdvanceService — Tạm ứng lương
    /// </summary>
    public class AdvanceServiceTests
    {
        private readonly Mock<AdvanceRepository> _mockAdvanceRepo;
        private readonly AdvanceService _sut;

        public AdvanceServiceTests()
        {
            _mockAdvanceRepo = new Mock<AdvanceRepository>(MockBehavior.Loose, new object[] { null! });
            _sut = new AdvanceService(_mockAdvanceRepo.Object);
        }

        // ===== CreateAsync Tests =====

        [Fact]
        public async Task CreateAsync_InvalidEmployeeId_Fails()
        {
            var advance = new Advance { EmployeeId = 0, Amount = 1_000_000m };
            var result = await _sut.CreateAsync(advance);
            Assert.False(result.Ok);
            Assert.Contains("chọn nhân viên", result.Msg);
        }

        [Fact]
        public async Task CreateAsync_ZeroAmount_Fails()
        {
            var advance = new Advance { EmployeeId = 1, Amount = 0 };
            var result = await _sut.CreateAsync(advance);
            Assert.False(result.Ok);
            Assert.Contains("Số tiền", result.Msg);
        }

        [Fact]
        public async Task CreateAsync_NegativeAmount_Fails()
        {
            var advance = new Advance { EmployeeId = 1, Amount = -500_000m };
            var result = await _sut.CreateAsync(advance);
            Assert.False(result.Ok);
        }

        [Fact]
        public async Task CreateAsync_ValidAdvance_Succeeds()
        {
            var advance = new Advance { EmployeeId = 1, Amount = 2_000_000m };
            _mockAdvanceRepo.Setup(r => r.InsertAsync(It.IsAny<Advance>())).ReturnsAsync(1);

            var result = await _sut.CreateAsync(advance);
            Assert.True(result.Ok);
            Assert.Equal("Pending", advance.Status);
        }

        // ===== DeleteAsync Tests =====

        [Fact]
        public async Task DeleteAsync_Success()
        {
            _mockAdvanceRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _sut.DeleteAsync(1);
            Assert.True(result.Ok);
        }
    }
}
