using Moq;
using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Tests
{
    /// <summary>
    /// Unit tests cho SalaryService — Thuế TNCN + Business Logic
    /// </summary>
    public class SalaryServiceTests
    {
        private readonly Mock<SalaryRepository> _mockSalaryRepo;
        private readonly Mock<AttendanceRepository> _mockAttendanceRepo;
        private readonly Mock<EmployeeRepository> _mockEmpRepo;
        private readonly Mock<PositionRepository> _mockPosRepo;
        private readonly Mock<AdvanceRepository> _mockAdvanceRepo;
        private readonly SalaryService _sut;

        public SalaryServiceTests()
        {
            _mockSalaryRepo = new Mock<SalaryRepository>(MockBehavior.Loose, new object[] { null! });
            _mockAttendanceRepo = new Mock<AttendanceRepository>(MockBehavior.Loose, new object[] { null! });
            _mockEmpRepo = new Mock<EmployeeRepository>(MockBehavior.Loose, new object[] { null! });
            _mockPosRepo = new Mock<PositionRepository>(MockBehavior.Loose, new object[] { null! });
            _mockAdvanceRepo = new Mock<AdvanceRepository>(MockBehavior.Loose, new object[] { null! });

            _sut = new SalaryService(
                _mockSalaryRepo.Object,
                _mockAttendanceRepo.Object,
                _mockEmpRepo.Object,
                _mockPosRepo.Object,
                _mockAdvanceRepo.Object);
        }

        // ===== CalculatePIT Tests (giữ nguyên) =====

        [Fact]
        public void CalculatePIT_ZeroIncome_ReturnsZero()
        {
            Assert.Equal(0, SalaryService.CalculatePIT(0));
        }

        [Fact]
        public void CalculatePIT_NegativeIncome_ReturnsZero()
        {
            Assert.Equal(0, SalaryService.CalculatePIT(-5_000_000m));
        }

        [Theory]
        [InlineData(1_000_000, 50_000)]           // 1M * 5% = 50K
        [InlineData(5_000_000, 250_000)]           // 5M * 5% = 250K (hết bậc 1)
        public void CalculatePIT_Bracket1_5Percent(decimal income, decimal expectedTax)
        {
            Assert.Equal(expectedTax, SalaryService.CalculatePIT(income));
        }

        [Theory]
        [InlineData(10_000_000, 750_000)]          // 5M*5% + 5M*10% = 750K
        [InlineData(7_000_000, 450_000)]           // 5M*5% + 2M*10% = 450K
        public void CalculatePIT_Bracket2_10Percent(decimal income, decimal expectedTax)
        {
            Assert.Equal(expectedTax, SalaryService.CalculatePIT(income));
        }

        [Theory]
        [InlineData(18_000_000, 1_950_000)]        // 250K + 500K + 8M*15%
        [InlineData(32_000_000, 4_750_000)]        // + 14M*20%
        [InlineData(52_000_000, 9_750_000)]        // + 20M*25%
        [InlineData(80_000_000, 18_150_000)]       // + 28M*30%
        public void CalculatePIT_HigherBrackets(decimal income, decimal expectedTax)
        {
            Assert.Equal(expectedTax, SalaryService.CalculatePIT(income));
        }

        [Fact]
        public void CalculatePIT_Bracket7_35Percent()
        {
            var tax = SalaryService.CalculatePIT(100_000_000m);
            Assert.Equal(25_150_000m, tax);
        }

        [Fact]
        public void CalculatePIT_SmallAmount_CorrectRounding()
        {
            Assert.Equal(175_000m, SalaryService.CalculatePIT(3_500_000m));
        }

        // ===== SaveConfigAsync Tests =====

        [Fact]
        public async Task SaveConfigAsync_EmptyCode_ReturnsFalse()
        {
            var config = new SalaryConfig { ConfigCode = "", ConfigName = "Test" };
            var result = await _sut.SaveConfigAsync(config);
            Assert.False(result.Ok);
            Assert.Contains("Mã cấu hình", result.Msg);
        }

        [Fact]
        public async Task SaveConfigAsync_EmptyName_ReturnsFalse()
        {
            var config = new SalaryConfig { ConfigCode = "TEST", ConfigName = "" };
            var result = await _sut.SaveConfigAsync(config);
            Assert.False(result.Ok);
            Assert.Contains("Tên cấu hình", result.Msg);
        }

        [Fact]
        public async Task SaveConfigAsync_Valid_ReturnsTrue()
        {
            var config = new SalaryConfig { ConfigCode = "TEST", ConfigName = "Test Config", ConfigValue = 100 };
            _mockSalaryRepo.Setup(r => r.UpsertConfigAsync(config)).Returns(Task.CompletedTask);

            var result = await _sut.SaveConfigAsync(config);
            Assert.True(result.Ok);
            _mockSalaryRepo.Verify(r => r.UpsertConfigAsync(config), Times.Once);
        }

        // ===== ApproveAsync Tests =====

        [Fact]
        public async Task ApproveAsync_NotFound_ReturnsFalse()
        {
            _mockSalaryRepo.Setup(r => r.GetByIdAsync(999L))
                .ReturnsAsync((SalaryRecord?)null);

            var result = await _sut.ApproveAsync(999L, 1);
            Assert.False(result.Ok);
            Assert.Contains("Không tìm thấy", result.Msg);
        }

        [Fact]
        public async Task ApproveAsync_AlreadyApproved_ReturnsFalse()
        {
            _mockSalaryRepo.Setup(r => r.GetByIdAsync(1L))
                .ReturnsAsync(new SalaryRecord { SalaryId = 1, Status = "Approved" });

            var result = await _sut.ApproveAsync(1L, 1);
            Assert.False(result.Ok);
            Assert.Contains("đã được duyệt", result.Msg);
        }

        [Fact]
        public async Task ApproveAsync_DraftRecord_ReturnsTrue()
        {
            _mockSalaryRepo.Setup(r => r.GetByIdAsync(1L))
                .ReturnsAsync(new SalaryRecord { SalaryId = 1, Status = "Draft" });
            _mockSalaryRepo.Setup(r => r.UpdateStatusAsync(1L, "Approved", 5))
                .Returns(Task.CompletedTask);

            var result = await _sut.ApproveAsync(1L, 5);
            Assert.True(result.Ok);
            _mockSalaryRepo.Verify(r => r.UpdateStatusAsync(1L, "Approved", 5), Times.Once);
        }

        // ===== UnapproveAsync Tests =====

        [Fact]
        public async Task UnapproveAsync_NotFound_ReturnsFalse()
        {
            _mockSalaryRepo.Setup(r => r.GetByIdAsync(999L))
                .ReturnsAsync((SalaryRecord?)null);

            var result = await _sut.UnapproveAsync(999L);
            Assert.False(result.Ok);
        }

        [Fact]
        public async Task UnapproveAsync_NotApproved_ReturnsFalse()
        {
            _mockSalaryRepo.Setup(r => r.GetByIdAsync(1L))
                .ReturnsAsync(new SalaryRecord { SalaryId = 1, Status = "Draft" });

            var result = await _sut.UnapproveAsync(1L);
            Assert.False(result.Ok);
            Assert.Contains("Chỉ có thể hủy duyệt", result.Msg);
        }

        [Fact]
        public async Task UnapproveAsync_Approved_ReturnsTrue()
        {
            _mockSalaryRepo.Setup(r => r.GetByIdAsync(1L))
                .ReturnsAsync(new SalaryRecord { SalaryId = 1, Status = "Approved" });
            _mockSalaryRepo.Setup(r => r.UpdateStatusAsync(1L, "Draft", null))
                .Returns(Task.CompletedTask);

            var result = await _sut.UnapproveAsync(1L);
            Assert.True(result.Ok);
        }

        // ===== CalculateForEmployeeAsync Tests =====

        [Fact]
        public async Task CalculateForEmployeeAsync_BasicScenario_CalculatesCorrectly()
        {
            // Arrange: NV lương cơ bản 10M, hệ số 1.0, 0 người phụ thuộc
            var emp = new Employee
            {
                EmployeeId = 1,
                FullName = "Nguyễn Văn A",
                EmployeeCode = "NV-0001",
                BasicSalary = 10_000_000m,
                SalaryCoefficient = 1.0m,
                PositionId = 1,
                NumberOfDependents = 0,
                DepartmentName = "IT"
            };

            // Config dictionary
            var configs = new Dictionary<string, decimal>
            {
                ["BHXH"] = 8m, // 8%
                ["BHYT"] = 1.5m, // 1.5%
                ["BHTN"] = 1m, // 1%
                ["GIAMTRU_BT"] = 11_000_000m,
                ["GIAMTRU_NPT"] = 4_400_000m,
                ["NGAY_CONG"] = 26m,
                ["OT_RATE"] = 1.5m,
                ["PC_ANTRUOI"] = 0m,
                ["PC_XANGXE"] = 0m,
                ["PC_DILAI"] = 0m,
                ["PHAT_DIMUON_MUC"] = 50_000m,
                ["PHAT_DIMUON_NGUONG"] = 3m
            };

            _mockSalaryRepo.Setup(r => r.GetConfigDictionaryAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(configs);

            // Position: không có phụ cấp
            _mockPosRepo.Setup(r => r.GetAllAsync(It.IsAny<bool>()))
                .ReturnsAsync(new List<Position> { new() { PositionId = 1, AllowanceAmount = 0 } });

            // 22 ngày công Present, không OT
            var attendanceRecords = Enumerable.Range(1, 22).Select(i => new AttendanceRecord
            {
                EmployeeId = 1,
                Status = "Present",
                OvertimeHours = 0
            }).ToList();

            _mockAttendanceRepo.Setup(r => r.GetMonthlyAsync(3, 2026, null))
                .ReturnsAsync(attendanceRecords);

            // Không tạm ứng
            _mockAdvanceRepo.Setup(r => r.GetTotalAdvanceAsync(1, 3, 2026))
                .ReturnsAsync(0m);

            // Act
            var result = await _sut.CalculateForEmployeeAsync(emp, 3, 2026);

            // Assert
            Assert.Equal(1, result.EmployeeId);
            Assert.Equal(3, result.Month);
            Assert.Equal(2026, result.Year);
            Assert.Equal(22, result.WorkingDays);
            Assert.Equal(10_000_000m, result.BasicSalary);
            Assert.True(result.GrossIncome > 0);
            Assert.True(result.SocialInsurance > 0); // 10M * 8% = 800K
            Assert.True(result.HealthInsurance > 0);
            Assert.True(result.UnemploymentInsurance > 0);
            Assert.Equal("Draft", result.Status);
        }

        // ===== CalculateMonthlyAsync Tests =====

        [Fact]
        public async Task CalculateMonthlyAsync_NoEmployees_ReturnsFalse()
        {
            _mockSalaryRepo.Setup(r => r.DeleteDraftByMonthAsync(3, 2026)).ReturnsAsync(0);
            _mockEmpRepo.Setup(r => r.GetAllAsync(1, 9999, null, null, true))
                .ReturnsAsync(new Models.DTOs.PaginationResult<Employee> { Items = new List<Employee>() });

            var result = await _sut.CalculateMonthlyAsync(3, 2026);
            Assert.False(result.Ok);
            Assert.Contains("Không có nhân viên", result.Msg);
        }

        // ===== ApproveAllAsync Tests =====

        [Fact]
        public async Task ApproveAllAsync_Success()
        {
            _mockSalaryRepo.Setup(r => r.BulkUpdateStatusAsync(3, 2026, "Approved", 1))
                .Returns(Task.CompletedTask);

            var result = await _sut.ApproveAllAsync(3, 2026, 1);
            Assert.True(result.Ok);
            _mockSalaryRepo.Verify(r => r.BulkUpdateStatusAsync(3, 2026, "Approved", 1), Times.Once);
        }
    }
}
