using Moq;
using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Tests
{
    /// <summary>
    /// Unit tests cho DepartmentService — Quản lý phòng ban
    /// </summary>
    public class DepartmentServiceTests
    {
        private readonly Mock<DepartmentRepository> _mockDeptRepo;
        private readonly DepartmentService _sut;

        public DepartmentServiceTests()
        {
            _mockDeptRepo = new Mock<DepartmentRepository>(MockBehavior.Loose, new object[] { null! });
            _sut = new DepartmentService(_mockDeptRepo.Object);
        }

        // ===== CreateAsync Tests =====

        [Fact]
        public async Task CreateAsync_EmptyName_Fails()
        {
            var dept = new Department { DepartmentName = "", DepartmentCode = "DEV" };
            var result = await _sut.CreateAsync(dept);
            Assert.False(result.Success);
            Assert.Contains("tên phòng ban", result.Message);
        }

        [Fact]
        public async Task CreateAsync_EmptyCode_Fails()
        {
            var dept = new Department { DepartmentName = "Phòng IT", DepartmentCode = "" };
            var result = await _sut.CreateAsync(dept);
            Assert.False(result.Success);
            Assert.Contains("mã phòng ban", result.Message);
        }

        [Fact]
        public async Task CreateAsync_Valid_Succeeds()
        {
            var dept = new Department { DepartmentName = "Phòng IT", DepartmentCode = "IT" };
            _mockDeptRepo.Setup(r => r.InsertAsync(dept)).ReturnsAsync(1);

            var result = await _sut.CreateAsync(dept);
            Assert.True(result.Success);
        }

        // ===== UpdateAsync Tests =====

        [Fact]
        public async Task UpdateAsync_EmptyName_Fails()
        {
            var dept = new Department { DepartmentId = 1, DepartmentName = "", DepartmentCode = "IT" };
            var result = await _sut.UpdateAsync(dept);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_Valid_Succeeds()
        {
            var dept = new Department { DepartmentId = 1, DepartmentName = "Phòng IT", DepartmentCode = "IT" };
            _mockDeptRepo.Setup(r => r.UpdateAsync(dept)).Returns(Task.CompletedTask);

            var result = await _sut.UpdateAsync(dept);
            Assert.True(result.Success);
        }

        // ===== DeleteAsync Tests =====

        [Fact]
        public async Task DeleteAsync_NotFound_Fails()
        {
            _mockDeptRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Department?)null);

            var result = await _sut.DeleteAsync(999);
            Assert.False(result.Success);
            Assert.Contains("Không tìm thấy", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_HasEmployees_Fails()
        {
            _mockDeptRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new Department { DepartmentId = 1, DepartmentName = "IT" });
            _mockDeptRepo.Setup(r => r.HasEmployeesAsync(1)).ReturnsAsync(true);

            var result = await _sut.DeleteAsync(1);
            Assert.False(result.Success);
            Assert.Contains("nhân viên", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_NoEmployees_Succeeds()
        {
            _mockDeptRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new Department { DepartmentId = 1, DepartmentName = "Empty" });
            _mockDeptRepo.Setup(r => r.HasEmployeesAsync(1)).ReturnsAsync(false);
            _mockDeptRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _sut.DeleteAsync(1);
            Assert.True(result.Success);
        }
    }
}
