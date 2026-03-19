using Moq;
using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.DTOs;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Tests
{
    /// <summary>
    /// Unit tests cho EmployeeService — Quản lý nhân viên
    /// </summary>
    public class EmployeeServiceTests
    {
        private readonly Mock<EmployeeRepository> _mockEmpRepo;
        private readonly Mock<DepartmentRepository> _mockDeptRepo;
        private readonly Mock<PositionRepository> _mockPosRepo;
        private readonly EmployeeService _sut;

        public EmployeeServiceTests()
        {
            _mockEmpRepo = new Mock<EmployeeRepository>(MockBehavior.Loose, new object[] { null! });
            _mockDeptRepo = new Mock<DepartmentRepository>(MockBehavior.Loose, new object[] { null! });
            _mockPosRepo = new Mock<PositionRepository>(MockBehavior.Loose, new object[] { null! });

            _sut = new EmployeeService(_mockEmpRepo.Object, _mockDeptRepo.Object, _mockPosRepo.Object);
        }

        // ===== CreateAsync Tests =====

        [Fact]
        public async Task CreateAsync_EmptyFullName_Fails()
        {
            var emp = new Employee { FullName = "", DepartmentId = 1, PositionId = 1 };
            var result = await _sut.CreateAsync(emp);
            Assert.False(result.Success);
            Assert.Contains("họ tên", result.Message);
        }

        [Fact]
        public async Task CreateAsync_InvalidDepartmentId_Fails()
        {
            var emp = new Employee { FullName = "Nguyễn Văn A", DepartmentId = 0, PositionId = 1 };
            var result = await _sut.CreateAsync(emp);
            Assert.False(result.Success);
            Assert.Contains("phòng ban", result.Message);
        }

        [Fact]
        public async Task CreateAsync_InvalidPositionId_Fails()
        {
            var emp = new Employee { FullName = "Nguyễn Văn A", DepartmentId = 1, PositionId = 0 };
            var result = await _sut.CreateAsync(emp);
            Assert.False(result.Success);
            Assert.Contains("chức vụ", result.Message);
        }

        [Fact]
        public async Task CreateAsync_ValidEmployee_Succeeds()
        {
            var emp = new Employee { FullName = "Nguyễn Văn A", DepartmentId = 1, PositionId = 1 };
            _mockEmpRepo.Setup(r => r.GenerateNextCodeAsync()).ReturnsAsync("NV-0005");
            _mockEmpRepo.Setup(r => r.InsertAsync(It.IsAny<Employee>())).ReturnsAsync(5);

            var result = await _sut.CreateAsync(emp);
            Assert.True(result.Success);
            Assert.Equal(5, result.EmployeeId);
            Assert.Equal("NV-0005", emp.EmployeeCode);
        }

        // ===== UpdateAsync Tests =====

        [Fact]
        public async Task UpdateAsync_EmptyFullName_Fails()
        {
            var emp = new Employee { EmployeeId = 1, FullName = "" };
            var result = await _sut.UpdateAsync(emp);
            Assert.False(result.Success);
            Assert.Contains("họ tên", result.Message);
        }

        [Fact]
        public async Task UpdateAsync_ValidEmployee_Succeeds()
        {
            var emp = new Employee { EmployeeId = 1, FullName = "Nguyễn Văn B" };
            _mockEmpRepo.Setup(r => r.UpdateAsync(emp)).Returns(Task.CompletedTask);

            var result = await _sut.UpdateAsync(emp);
            Assert.True(result.Success);
            Assert.NotNull(emp.UpdatedAt);
        }

        // ===== DeleteAsync Tests =====

        [Fact]
        public async Task DeleteAsync_NotFound_Fails()
        {
            _mockEmpRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Employee?)null);

            var result = await _sut.DeleteAsync(999);
            Assert.False(result.Success);
            Assert.Contains("Không tìm thấy", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_Found_Succeeds()
        {
            _mockEmpRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Employee { EmployeeId = 1 });
            _mockEmpRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _sut.DeleteAsync(1);
            Assert.True(result.Success);
        }

        // ===== BatchUpdateAsync Tests =====

        [Fact]
        public async Task BatchUpdateAsync_EmptyList_Fails()
        {
            var result = await _sut.BatchUpdateAsync(new List<int>(), "DepartmentId", 1);
            Assert.False(result.Success);
            Assert.Contains("Chưa chọn", result.Message);
        }

        [Fact]
        public async Task BatchUpdateAsync_NullList_Fails()
        {
            var result = await _sut.BatchUpdateAsync(null!, "DepartmentId", 1);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task BatchUpdateAsync_ValidUpdate_Succeeds()
        {
            var ids = new List<int> { 1, 2, 3 };
            _mockEmpRepo.Setup(r => r.BatchUpdateFieldAsync(ids, "DepartmentId", 5)).ReturnsAsync(3);

            var result = await _sut.BatchUpdateAsync(ids, "DepartmentId", 5);
            Assert.True(result.Success);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task BatchUpdateAsync_DisallowedField_Fails()
        {
            var ids = new List<int> { 1 };
            _mockEmpRepo.Setup(r => r.BatchUpdateFieldAsync(ids, "PasswordHash", "evil"))
                .ThrowsAsync(new ArgumentException("Trường 'PasswordHash' không được phép cập nhật hàng loạt."));

            var result = await _sut.BatchUpdateAsync(ids, "PasswordHash", "evil");
            Assert.False(result.Success);
            Assert.Contains("PasswordHash", result.Message);
        }
    }
}
