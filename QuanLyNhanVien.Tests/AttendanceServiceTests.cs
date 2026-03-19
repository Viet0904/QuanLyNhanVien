using Moq;
using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Tests
{
    /// <summary>
    /// Unit tests cho AttendanceService — Chấm công, DetermineStatus, CalculateOvertime
    /// </summary>
    public class AttendanceServiceTests
    {
        private readonly Mock<AttendanceRepository> _mockAttendanceRepo;
        private readonly Mock<EmployeeRepository> _mockEmpRepo;
        private readonly AttendanceService _sut;

        public AttendanceServiceTests()
        {
            _mockAttendanceRepo = new Mock<AttendanceRepository>(MockBehavior.Loose, new object[] { null! });
            _mockEmpRepo = new Mock<EmployeeRepository>(MockBehavior.Loose, new object[] { null! });

            _sut = new AttendanceService(_mockAttendanceRepo.Object, _mockEmpRepo.Object);
        }

        // ===== AddAsync Tests =====

        [Fact]
        public async Task AddAsync_InvalidEmployeeId_Fails()
        {
            var record = new AttendanceRecord { EmployeeId = 0 };
            var result = await _sut.AddAsync(record);
            Assert.False(result.Success);
            Assert.Contains("chọn nhân viên", result.Message);
        }

        [Fact]
        public async Task AddAsync_DuplicateRecord_Fails()
        {
            var record = new AttendanceRecord
            {
                EmployeeId = 1,
                WorkDate = new DateTime(2026, 3, 16)
            };

            _mockAttendanceRepo.Setup(r => r.GetByEmployeeAndDateAsync(1, record.WorkDate))
                .ReturnsAsync(new AttendanceRecord { RecordId = 99 }); // đã tồn tại

            var result = await _sut.AddAsync(record);
            Assert.False(result.Success);
            Assert.Contains("đã được chấm công", result.Message);
        }

        [Fact]
        public async Task AddAsync_ValidRecord_ReturnsSuccess()
        {
            var record = new AttendanceRecord
            {
                EmployeeId = 1,
                WorkDate = new DateTime(2026, 3, 16),
                ShiftType = "FULLDAY",
                CheckIn = new TimeSpan(7, 30, 0),
                CheckOut = new TimeSpan(17, 0, 0)
            };

            _mockAttendanceRepo.Setup(r => r.GetByEmployeeAndDateAsync(1, record.WorkDate))
                .ReturnsAsync((AttendanceRecord?)null);
            _mockAttendanceRepo.Setup(r => r.InsertAsync(It.IsAny<AttendanceRecord>()))
                .ReturnsAsync(1L);

            var result = await _sut.AddAsync(record);
            Assert.True(result.Success);
            Assert.Equal("Present", record.Status);
            Assert.Equal(0, record.OvertimeHours);
        }

        // ===== DetermineStatus Tests (thông qua AddAsync) =====

        [Fact]
        public async Task AddAsync_LateCheckIn_StatusIsLate()
        {
            // Ca FULLDAY bắt đầu 7:30, trễ nếu > 7:45
            var record = new AttendanceRecord
            {
                EmployeeId = 1,
                WorkDate = new DateTime(2026, 3, 16),
                ShiftType = "FULLDAY",
                CheckIn = new TimeSpan(8, 0, 0), // trễ 30 phút
                CheckOut = new TimeSpan(17, 0, 0)
            };

            _mockAttendanceRepo.Setup(r => r.GetByEmployeeAndDateAsync(1, record.WorkDate))
                .ReturnsAsync((AttendanceRecord?)null);
            _mockAttendanceRepo.Setup(r => r.InsertAsync(It.IsAny<AttendanceRecord>()))
                .ReturnsAsync(1L);

            await _sut.AddAsync(record);
            Assert.Equal("Late", record.Status);
        }

        [Fact]
        public async Task AddAsync_EarlyLeave_StatusIsEarlyLeave()
        {
            // Ca FULLDAY kết thúc 17:00, về sớm nếu < 16:45
            var record = new AttendanceRecord
            {
                EmployeeId = 1,
                WorkDate = new DateTime(2026, 3, 16),
                ShiftType = "FULLDAY",
                CheckIn = new TimeSpan(7, 0, 0), // đúng giờ
                CheckOut = new TimeSpan(16, 0, 0) // về sớm 1 tiếng
            };

            _mockAttendanceRepo.Setup(r => r.GetByEmployeeAndDateAsync(1, record.WorkDate))
                .ReturnsAsync((AttendanceRecord?)null);
            _mockAttendanceRepo.Setup(r => r.InsertAsync(It.IsAny<AttendanceRecord>()))
                .ReturnsAsync(1L);

            await _sut.AddAsync(record);
            Assert.Equal("EarlyLeave", record.Status);
        }

        [Fact]
        public async Task AddAsync_LateAndEarlyLeave_StatusIsLateAndEarlyLeave()
        {
            var record = new AttendanceRecord
            {
                EmployeeId = 1,
                WorkDate = new DateTime(2026, 3, 16),
                ShiftType = "FULLDAY",
                CheckIn = new TimeSpan(8, 0, 0),  // trễ
                CheckOut = new TimeSpan(16, 0, 0)  // về sớm
            };

            _mockAttendanceRepo.Setup(r => r.GetByEmployeeAndDateAsync(1, record.WorkDate))
                .ReturnsAsync((AttendanceRecord?)null);
            _mockAttendanceRepo.Setup(r => r.InsertAsync(It.IsAny<AttendanceRecord>()))
                .ReturnsAsync(1L);

            await _sut.AddAsync(record);
            Assert.Equal("LateAndEarlyLeave", record.Status);
        }

        [Fact]
        public async Task AddAsync_OnTime_StatusIsPresent()
        {
            // Đúng giờ: check-in <= 7:45, check-out >= 16:45
            var record = new AttendanceRecord
            {
                EmployeeId = 1,
                WorkDate = new DateTime(2026, 3, 16),
                ShiftType = "FULLDAY",
                CheckIn = new TimeSpan(7, 20, 0),
                CheckOut = new TimeSpan(17, 30, 0)
            };

            _mockAttendanceRepo.Setup(r => r.GetByEmployeeAndDateAsync(1, record.WorkDate))
                .ReturnsAsync((AttendanceRecord?)null);
            _mockAttendanceRepo.Setup(r => r.InsertAsync(It.IsAny<AttendanceRecord>()))
                .ReturnsAsync(1L);

            await _sut.AddAsync(record);
            Assert.Equal("Present", record.Status);
        }

        // ===== Overtime Tests =====

        [Fact]
        public async Task AddAsync_WithOvertime_CalculatesCorrectly()
        {
            // Ca FULLDAY kết thúc 17:00, checkout 19:00 → 2h OT
            var record = new AttendanceRecord
            {
                EmployeeId = 1,
                WorkDate = new DateTime(2026, 3, 16),
                ShiftType = "FULLDAY",
                CheckIn = new TimeSpan(7, 30, 0),
                CheckOut = new TimeSpan(19, 0, 0) // 2h tăng ca
            };

            _mockAttendanceRepo.Setup(r => r.GetByEmployeeAndDateAsync(1, record.WorkDate))
                .ReturnsAsync((AttendanceRecord?)null);
            _mockAttendanceRepo.Setup(r => r.InsertAsync(It.IsAny<AttendanceRecord>()))
                .ReturnsAsync(1L);

            await _sut.AddAsync(record);
            Assert.Equal(2m, record.OvertimeHours);
        }

        [Fact]
        public async Task AddAsync_NoOvertime_ReturnsZero()
        {
            var record = new AttendanceRecord
            {
                EmployeeId = 1,
                WorkDate = new DateTime(2026, 3, 16),
                ShiftType = "FULLDAY",
                CheckIn = new TimeSpan(7, 30, 0),
                CheckOut = new TimeSpan(16, 30, 0) // về trước giờ kết thúc
            };

            _mockAttendanceRepo.Setup(r => r.GetByEmployeeAndDateAsync(1, record.WorkDate))
                .ReturnsAsync((AttendanceRecord?)null);
            _mockAttendanceRepo.Setup(r => r.InsertAsync(It.IsAny<AttendanceRecord>()))
                .ReturnsAsync(1L);

            await _sut.AddAsync(record);
            Assert.Equal(0m, record.OvertimeHours);
        }

        // ===== Morning/Afternoon Shift Tests =====

        [Fact]
        public async Task AddAsync_MorningShift_LateCheckIn()
        {
            // Ca sáng: 7:30 - 11:30, trễ nếu > 7:45
            var record = new AttendanceRecord
            {
                EmployeeId = 1,
                WorkDate = new DateTime(2026, 3, 16),
                ShiftType = "MORNING",
                CheckIn = new TimeSpan(7, 50, 0),
                CheckOut = new TimeSpan(11, 30, 0)
            };

            _mockAttendanceRepo.Setup(r => r.GetByEmployeeAndDateAsync(1, record.WorkDate))
                .ReturnsAsync((AttendanceRecord?)null);
            _mockAttendanceRepo.Setup(r => r.InsertAsync(It.IsAny<AttendanceRecord>()))
                .ReturnsAsync(1L);

            await _sut.AddAsync(record);
            Assert.Equal("Late", record.Status);
        }

        [Fact]
        public async Task AddAsync_AfternoonShift_OnTime()
        {
            // Ca chiều: 13:00 - 17:00
            var record = new AttendanceRecord
            {
                EmployeeId = 1,
                WorkDate = new DateTime(2026, 3, 16),
                ShiftType = "AFTERNOON",
                CheckIn = new TimeSpan(12, 50, 0),
                CheckOut = new TimeSpan(17, 0, 0)
            };

            _mockAttendanceRepo.Setup(r => r.GetByEmployeeAndDateAsync(1, record.WorkDate))
                .ReturnsAsync((AttendanceRecord?)null);
            _mockAttendanceRepo.Setup(r => r.InsertAsync(It.IsAny<AttendanceRecord>()))
                .ReturnsAsync(1L);

            await _sut.AddAsync(record);
            Assert.Equal("Present", record.Status);
        }

        // ===== UpdateAsync Tests =====

        [Fact]
        public async Task UpdateAsync_RecalculatesStatusAndOvertime()
        {
            var record = new AttendanceRecord
            {
                RecordId = 1,
                EmployeeId = 1,
                ShiftType = "FULLDAY",
                CheckIn = new TimeSpan(8, 0, 0),   // trễ
                CheckOut = new TimeSpan(19, 0, 0)   // 2h OT
            };

            _mockAttendanceRepo.Setup(r => r.UpdateAsync(record)).Returns(Task.CompletedTask);

            var result = await _sut.UpdateAsync(record);
            Assert.True(result.Success);
            Assert.Equal("Late", record.Status);
            Assert.Equal(2m, record.OvertimeHours);
        }

        // ===== DeleteAsync Tests =====

        [Fact]
        public async Task DeleteAsync_Success()
        {
            _mockAttendanceRepo.Setup(r => r.DeleteAsync(1L)).Returns(Task.CompletedTask);

            var result = await _sut.DeleteAsync(1);
            Assert.True(result.Success);
        }
    }
}
