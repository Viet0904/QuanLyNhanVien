using Moq;
using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Tests
{
    /// <summary>
    /// Unit tests cho PositionService — Quản lý chức vụ
    /// </summary>
    public class PositionServiceTests
    {
        private readonly Mock<PositionRepository> _mockPosRepo;
        private readonly PositionService _sut;

        public PositionServiceTests()
        {
            _mockPosRepo = new Mock<PositionRepository>(MockBehavior.Loose, new object[] { null! });
            _sut = new PositionService(_mockPosRepo.Object);
        }

        // ===== CreateAsync Tests =====

        [Fact]
        public async Task CreateAsync_EmptyName_Fails()
        {
            var pos = new Position { PositionName = "" };
            var result = await _sut.CreateAsync(pos);
            Assert.False(result.Success);
            Assert.Contains("tên chức vụ", result.Message);
        }

        [Fact]
        public async Task CreateAsync_ValidPosition_Succeeds()
        {
            var pos = new Position { PositionName = "Trưởng phòng", Level = 3, AllowanceAmount = 1_000_000m };
            _mockPosRepo.Setup(r => r.InsertAsync(pos)).ReturnsAsync(1);

            var result = await _sut.CreateAsync(pos);
            Assert.True(result.Success);
        }

        // ===== UpdateAsync Tests =====

        [Fact]
        public async Task UpdateAsync_EmptyName_Fails()
        {
            var pos = new Position { PositionId = 1, PositionName = "" };
            var result = await _sut.UpdateAsync(pos);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_Valid_Succeeds()
        {
            var pos = new Position { PositionId = 1, PositionName = "Phó phòng" };
            _mockPosRepo.Setup(r => r.UpdateAsync(pos)).Returns(Task.CompletedTask);

            var result = await _sut.UpdateAsync(pos);
            Assert.True(result.Success);
        }

        // ===== DeleteAsync Tests =====

        [Fact]
        public async Task DeleteAsync_HasEmployees_Fails()
        {
            _mockPosRepo.Setup(r => r.HasEmployeesAsync(1)).ReturnsAsync(true);

            var result = await _sut.DeleteAsync(1);
            Assert.False(result.Success);
            Assert.Contains("nhân viên", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_NoEmployees_Succeeds()
        {
            _mockPosRepo.Setup(r => r.HasEmployeesAsync(1)).ReturnsAsync(false);
            _mockPosRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _sut.DeleteAsync(1);
            Assert.True(result.Success);
        }
    }
}
