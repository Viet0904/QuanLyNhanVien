using Moq;
using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Tests
{
    /// <summary>
    /// Unit tests cho ContractService — Hợp đồng lao động
    /// </summary>
    public class ContractServiceTests
    {
        private readonly Mock<ContractRepository> _mockContractRepo;
        private readonly Mock<EmployeeRepository> _mockEmpRepo;
        private readonly ContractService _sut;

        public ContractServiceTests()
        {
            _mockContractRepo = new Mock<ContractRepository>(MockBehavior.Loose, new object[] { null! });
            _mockEmpRepo = new Mock<EmployeeRepository>(MockBehavior.Loose, new object[] { null! });
            _sut = new ContractService(_mockContractRepo.Object, _mockEmpRepo.Object);
        }

        // ===== CreateAsync Tests =====

        [Fact]
        public async Task CreateAsync_InvalidEmployeeId_Fails()
        {
            var contract = new Contract { EmployeeId = 0, ContractType = "CT_XDTH" };
            var result = await _sut.CreateAsync(contract);
            Assert.False(result.Ok);
            Assert.Contains("chọn nhân viên", result.Msg);
        }

        [Fact]
        public async Task CreateAsync_EmptyContractType_Fails()
        {
            var contract = new Contract { EmployeeId = 1, ContractType = "" };
            var result = await _sut.CreateAsync(contract);
            Assert.False(result.Ok);
            Assert.Contains("loại hợp đồng", result.Msg);
        }

        [Fact]
        public async Task CreateAsync_ValidContract_Succeeds()
        {
            var contract = new Contract { EmployeeId = 1, ContractType = "CT_XDTH" };
            _mockContractRepo.Setup(r => r.InsertAsync(It.IsAny<Contract>()))
                .ReturnsAsync(10);

            var result = await _sut.CreateAsync(contract);
            Assert.True(result.Ok);
            Assert.StartsWith("HD-", contract.ContractCode);
            _mockContractRepo.Verify(r => r.InsertAsync(It.IsAny<Contract>()), Times.Once);
        }

        // ===== DeactivateExpiredEmployeesAsync Tests =====

        [Fact]
        public async Task DeactivateExpiredEmployeesAsync_NoExpired_ReturnsZero()
        {
            _mockContractRepo.Setup(r => r.GetExpiredActiveAsync())
                .ReturnsAsync(new List<Contract>());

            var result = await _sut.DeactivateExpiredEmployeesAsync();
            Assert.True(result.Ok);
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public async Task DeactivateExpiredEmployeesAsync_HasExpired_DeactivatesAll()
        {
            var expired = new List<Contract>
            {
                new() { ContractId = 1, EmployeeId = 10, IsActive = true },
                new() { ContractId = 2, EmployeeId = 20, IsActive = true }
            };

            _mockContractRepo.Setup(r => r.GetExpiredActiveAsync()).ReturnsAsync(expired);
            _mockEmpRepo.Setup(r => r.DeactivateAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
            _mockContractRepo.Setup(r => r.UpdateAsync(It.IsAny<Contract>())).Returns(Task.CompletedTask);

            var result = await _sut.DeactivateExpiredEmployeesAsync();
            Assert.True(result.Ok);
            Assert.Equal(2, result.Count);
            _mockEmpRepo.Verify(r => r.DeactivateAsync(10), Times.Once);
            _mockEmpRepo.Verify(r => r.DeactivateAsync(20), Times.Once);
        }

        // ===== UpdateAsync / DeleteAsync Tests =====

        [Fact]
        public async Task UpdateAsync_ReturnsSuccess()
        {
            var contract = new Contract { ContractId = 1, EmployeeId = 1, ContractType = "CT_KXDTH" };
            _mockContractRepo.Setup(r => r.UpdateAsync(contract)).Returns(Task.CompletedTask);

            var result = await _sut.UpdateAsync(contract);
            Assert.True(result.Ok);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsSuccess()
        {
            _mockContractRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _sut.DeleteAsync(1);
            Assert.True(result.Ok);
        }
    }
}
