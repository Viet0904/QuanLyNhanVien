using Moq;
using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.DTOs;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Tests
{
    /// <summary>
    /// Unit tests cho LeaveService — Nghỉ phép
    /// </summary>
    public class LeaveServiceTests
    {
        private readonly Mock<LeaveRequestRepository> _mockLeaveRepo;
        private readonly Mock<HolidayRepository> _mockHolidayRepo;
        private readonly Mock<AttendanceRepository> _mockAttendanceRepo;
        private readonly LeaveService _sut;

        public LeaveServiceTests()
        {
            _mockLeaveRepo = new Mock<LeaveRequestRepository>(MockBehavior.Loose, new object[] { null! });
            _mockHolidayRepo = new Mock<HolidayRepository>(MockBehavior.Loose, new object[] { null! });
            _mockAttendanceRepo = new Mock<AttendanceRepository>(MockBehavior.Loose, new object[] { null! });

            _sut = new LeaveService(
                _mockLeaveRepo.Object,
                _mockHolidayRepo.Object,
                _mockAttendanceRepo.Object);
        }

        // ===== CreateAsync Tests =====

        [Fact]
        public async Task CreateAsync_InvalidEmployeeId_Fails()
        {
            var req = new LeaveRequest { EmployeeId = 0, StartDate = DateTime.Today, EndDate = DateTime.Today };
            var result = await _sut.CreateAsync(req);
            Assert.False(result.Success);
            Assert.Contains("chọn nhân viên", result.Message);
        }

        [Fact]
        public async Task CreateAsync_StartAfterEnd_Fails()
        {
            var req = new LeaveRequest
            {
                EmployeeId = 1,
                StartDate = new DateTime(2026, 3, 20),
                EndDate = new DateTime(2026, 3, 18),
                LeaveType = "Annual"
            };
            var result = await _sut.CreateAsync(req);
            Assert.False(result.Success);
            Assert.Contains("Ngày bắt đầu", result.Message);
        }

        [Fact]
        public async Task CreateAsync_EmptyLeaveType_Fails()
        {
            var req = new LeaveRequest
            {
                EmployeeId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                LeaveType = ""
            };
            var result = await _sut.CreateAsync(req);
            Assert.False(result.Success);
            Assert.Contains("loại nghỉ phép", result.Message);
        }

        [Fact]
        public async Task CreateAsync_OverlappingLeave_Fails()
        {
            var req = new LeaveRequest
            {
                EmployeeId = 1,
                StartDate = new DateTime(2026, 3, 10),
                EndDate = new DateTime(2026, 3, 12),
                LeaveType = "Sick"
            };

            _mockLeaveRepo.Setup(r => r.HasOverlappingLeaveAsync(1, req.StartDate, req.EndDate, null))
                .ReturnsAsync(true);

            var result = await _sut.CreateAsync(req);
            Assert.False(result.Success);
            Assert.Contains("trùng khoảng ngày", result.Message);
        }

        [Fact]
        public async Task CreateAsync_ExceedsAnnualLeave_Fails()
        {
            var req = new LeaveRequest
            {
                EmployeeId = 1,
                StartDate = new DateTime(2026, 3, 10),
                EndDate = new DateTime(2026, 3, 14),
                LeaveType = "Annual"
            };

            _mockLeaveRepo.Setup(r => r.HasOverlappingLeaveAsync(1, req.StartDate, req.EndDate, null))
                .ReturnsAsync(false);
            _mockHolidayRepo.Setup(r => r.GetHolidayDatesAsync(req.StartDate, req.EndDate))
                .ReturnsAsync(new HashSet<DateTime>());
            // Đã dùng 10 ngày, xin thêm 5 ngày (Mon-Fri) > còn 2 ngày
            _mockLeaveRepo.Setup(r => r.GetUsedLeaveDaysAsync(1, 2026))
                .ReturnsAsync(10m);

            var result = await _sut.CreateAsync(req);
            Assert.False(result.Success);
            Assert.Contains("Vượt quá", result.Message);
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_Success()
        {
            var req = new LeaveRequest
            {
                EmployeeId = 1,
                StartDate = new DateTime(2026, 3, 16), // Monday
                EndDate = new DateTime(2026, 3, 17),   // Tuesday
                LeaveType = "Sick"
            };

            _mockLeaveRepo.Setup(r => r.HasOverlappingLeaveAsync(1, req.StartDate, req.EndDate, null))
                .ReturnsAsync(false);
            _mockHolidayRepo.Setup(r => r.GetHolidayDatesAsync(req.StartDate, req.EndDate))
                .ReturnsAsync(new HashSet<DateTime>());
            _mockLeaveRepo.Setup(r => r.InsertAsync(It.IsAny<LeaveRequest>()))
                .ReturnsAsync(42);

            var result = await _sut.CreateAsync(req);
            Assert.True(result.Success);
            Assert.Equal(42, result.LeaveId);
            Assert.Equal("Pending", req.Status);
        }

        // ===== ApproveAsync Tests =====

        [Fact]
        public async Task ApproveAsync_NotFound_Fails()
        {
            _mockLeaveRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((LeaveRequest?)null);

            var result = await _sut.ApproveAsync(999, 1);
            Assert.False(result.Success);
            Assert.Contains("Không tìm thấy", result.Message);
        }

        [Fact]
        public async Task ApproveAsync_AlreadyApproved_Fails()
        {
            _mockLeaveRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new LeaveRequest { LeaveId = 1, Status = "Approved" });

            var result = await _sut.ApproveAsync(1, 1);
            Assert.False(result.Success);
            Assert.Contains("Approved", result.Message);
        }

        [Fact]
        public async Task ApproveAsync_PendingRequest_Succeeds()
        {
            var leave = new LeaveRequest
            {
                LeaveId = 1,
                EmployeeId = 5,
                Status = "Pending",
                StartDate = new DateTime(2026, 3, 16),  // Monday
                EndDate = new DateTime(2026, 3, 17),     // Tuesday
                LeaveType = "Annual",
                EmployeeName = "Nguyễn Văn B"
            };

            _mockLeaveRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(leave);
            _mockLeaveRepo.Setup(r => r.UpdateStatusAsync(1, "Approved", 10)).Returns(Task.CompletedTask);
            _mockHolidayRepo.Setup(r => r.GetHolidayDatesAsync(leave.StartDate, leave.EndDate))
                .ReturnsAsync(new HashSet<DateTime>());
            _mockAttendanceRepo.Setup(r => r.GetByEmployeeAndDateAsync(5, It.IsAny<DateTime>()))
                .ReturnsAsync((AttendanceRecord?)null);
            _mockAttendanceRepo.Setup(r => r.InsertAsync(It.IsAny<AttendanceRecord>()))
                .ReturnsAsync(1L);

            var result = await _sut.ApproveAsync(1, 10);
            Assert.True(result.Success);
            _mockLeaveRepo.Verify(r => r.UpdateStatusAsync(1, "Approved", 10), Times.Once);
        }

        // ===== RejectAsync Tests =====

        [Fact]
        public async Task RejectAsync_NotFound_Fails()
        {
            _mockLeaveRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((LeaveRequest?)null);

            var result = await _sut.RejectAsync(999, 1);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task RejectAsync_NotPending_Fails()
        {
            _mockLeaveRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new LeaveRequest { LeaveId = 1, Status = "Approved" });

            var result = await _sut.RejectAsync(1, 1);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task RejectAsync_Pending_Succeeds()
        {
            _mockLeaveRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new LeaveRequest { LeaveId = 1, Status = "Pending", EmployeeName = "A" });
            _mockLeaveRepo.Setup(r => r.UpdateStatusAsync(1, "Rejected", 5)).Returns(Task.CompletedTask);

            var result = await _sut.RejectAsync(1, 5);
            Assert.True(result.Success);
        }

        // ===== DeleteAsync Tests =====

        [Fact]
        public async Task DeleteAsync_NotFound_Fails()
        {
            _mockLeaveRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((LeaveRequest?)null);

            var result = await _sut.DeleteAsync(999);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_NotPending_Fails()
        {
            _mockLeaveRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new LeaveRequest { LeaveId = 1, Status = "Approved" });

            var result = await _sut.DeleteAsync(1);
            Assert.False(result.Success);
            Assert.Contains("Đang chờ duyệt", result.Message);
        }

        [Fact]
        public async Task DeleteAsync_Pending_Succeeds()
        {
            _mockLeaveRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new LeaveRequest { LeaveId = 1, Status = "Pending" });
            _mockLeaveRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _sut.DeleteAsync(1);
            Assert.True(result.Success);
        }
    }
}
