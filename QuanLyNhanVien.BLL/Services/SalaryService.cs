using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.BLL.Services
{
    public class SalaryService
    {
        private readonly SalaryRepository _salaryRepo;
        private readonly AttendanceRepository _attendanceRepo;
        private readonly EmployeeRepository _empRepo;
        private readonly PositionRepository _posRepo;

        public SalaryService(SalaryRepository salaryRepo, AttendanceRepository attendanceRepo,
                             EmployeeRepository empRepo, PositionRepository posRepo)
        {
            _salaryRepo = salaryRepo;
            _attendanceRepo = attendanceRepo;
            _empRepo = empRepo;
            _posRepo = posRepo;
        }

        // ===== CONFIG =====

        public Task<IEnumerable<SalaryConfig>> GetAllConfigsAsync() => _salaryRepo.GetAllConfigsAsync();

        public async Task<(bool Ok, string Msg)> SaveConfigAsync(SalaryConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.ConfigCode))
                return (false, "Mã cấu hình không được để trống.");
            if (string.IsNullOrWhiteSpace(config.ConfigName))
                return (false, "Tên cấu hình không được để trống.");

            await _salaryRepo.UpsertConfigAsync(config);
            return (true, "Lưu cấu hình thành công!");
        }

        // ===== TÍNH LƯƠNG =====

        /// <summary>
        /// Lấy giá trị config, trả về 0 nếu không tìm thấy
        /// </summary>
        private async Task<decimal> GetConfigValueAsync(string code)
        {
            var config = await _salaryRepo.GetConfigByCodeAsync(code);
            return config?.ConfigValue ?? 0;
        }

        /// <summary>
        /// Tính lương cho 1 nhân viên trong 1 tháng
        /// </summary>
        public async Task<SalaryRecord> CalculateForEmployeeAsync(Employee emp, int month, int year)
        {
            // 1. Lấy configs bảo hiểm & thuế
            var bhxhRate = await GetConfigValueAsync("BHXH") / 100;
            var bhytRate = await GetConfigValueAsync("BHYT") / 100;
            var bhtnRate = await GetConfigValueAsync("BHTN") / 100;
            var personalDeduction = await GetConfigValueAsync("GIAMTRU_BT");
            var dependentDeduction = await GetConfigValueAsync("GIAMTRU_NPT");
            var standardDays = await GetConfigValueAsync("NGAY_CONG");
            var otRate = await GetConfigValueAsync("OT_RATE");
            if (standardDays <= 0) standardDays = 26;
            if (otRate <= 0) otRate = 1.5m;

            // 2. Lấy phụ cấp chức vụ
            var positions = await _posRepo.GetAllAsync();
            var position = positions.FirstOrDefault(p => p.PositionId == emp.PositionId);
            var positionAllowance = position?.AllowanceAmount ?? 0;

            // 3. Lấy phụ cấp khác (ăn trưa, xăng xe, đi lại) từ SalaryConfigs
            var lunchAllowance = await GetConfigValueAsync("PC_ANTRUOI");
            var petrolAllowance = await GetConfigValueAsync("PC_XANGXE");
            var travelAllowance = await GetConfigValueAsync("PC_DILAI");
            var otherAllowance = lunchAllowance + petrolAllowance + travelAllowance;

            // 4. Đếm ngày công từ chấm công
            var attendanceRecords = await _attendanceRepo.GetMonthlyAsync(month, year);
            var empAttendance = attendanceRecords.Where(a => a.EmployeeId == emp.EmployeeId).ToList();
            // Chỉ tính ngày có mặt, đi trễ, về sớm, nghỉ phép CÓ LƯƠNG
            // Nghỉ không lương (UnpaidLeave) KHÔNG được tính lương
            var workingDays = empAttendance.Count(a =>
                a.Status == "Present" || a.Status == "Late" || a.Status == "EarlyLeave" || a.Status == "OnLeave");
            var totalOvertimeHours = empAttendance.Sum(a => a.OvertimeHours);

            // 5. Tính khấu trừ phạt đi muộn
            var latePenaltyAmount = await GetConfigValueAsync("PHAT_DIMUON_MUC");   // VNĐ/lần
            var latePenaltyThreshold = await GetConfigValueAsync("PHAT_DIMUON_NGUONG"); // Số lần miễn phạt
            var lateCount = empAttendance.Count(a => a.Status == "Late" || a.Status == "EarlyLeave");
            var penalizableLateCount = Math.Max(0, lateCount - (int)latePenaltyThreshold);
            var otherDeductions = penalizableLateCount * latePenaltyAmount;

            // 6. Tính thu nhập
            var basicSalary = emp.BasicSalary;
            var coefficient = emp.SalaryCoefficient;
            var dailyRate = (basicSalary * coefficient) / standardDays;
            var grossFromWork = dailyRate * workingDays;
            var overtimePay = (dailyRate / 8) * totalOvertimeHours * otRate; // OT = daily/8 * hours * rate
            var grossIncome = grossFromWork + positionAllowance + otherAllowance + overtimePay;

            // 7. Tính bảo hiểm (trên lương cơ bản × hệ số)
            var insuranceBase = basicSalary * coefficient;
            var socialIns = Math.Round(insuranceBase * bhxhRate);
            var healthIns = Math.Round(insuranceBase * bhytRate);
            var unemploymentIns = Math.Round(insuranceBase * bhtnRate);
            var totalInsurance = socialIns + healthIns + unemploymentIns;

            // 8. Tính thuế TNCN
            var depDeduction = dependentDeduction * emp.NumberOfDependents;
            var taxableIncome = grossIncome - totalInsurance - personalDeduction - depDeduction;
            if (taxableIncome < 0) taxableIncome = 0;
            var pit = CalculatePIT(taxableIncome);

            // 9. Lương thực lĩnh
            var netSalary = grossIncome - totalInsurance - pit - otherDeductions;

            return new SalaryRecord
            {
                EmployeeId = emp.EmployeeId,
                Month = month,
                Year = year,
                WorkingDays = workingDays,
                StandardDays = standardDays,
                BasicSalary = basicSalary,
                SalaryCoefficient = coefficient,
                PositionAllowance = positionAllowance,
                OtherAllowance = Math.Round(otherAllowance),
                OvertimePay = Math.Round(overtimePay),
                GrossIncome = Math.Round(grossIncome),
                SocialInsurance = socialIns,
                HealthInsurance = healthIns,
                UnemploymentInsurance = unemploymentIns,
                PersonalDeduction = personalDeduction,
                DependentDeduction = depDeduction,
                TaxableIncome = taxableIncome,
                PersonalIncomeTax = pit,
                OtherDeductions = Math.Round(otherDeductions),
                NetSalary = Math.Round(netSalary),
                Status = "Draft",
                EmployeeName = emp.FullName,
                EmployeeCode = emp.EmployeeCode,
                DepartmentName = emp.DepartmentName
            };
        }

        /// <summary>
        /// Tính lương hàng loạt cho tất cả NV (hoặc theo phòng ban)
        /// </summary>
        public async Task<(bool Ok, string Msg, int Count)> CalculateMonthlyAsync(int month, int year, int? deptId = null)
        {
            // Xóa phiếu lương Draft cũ của tháng đó
            await _salaryRepo.DeleteDraftByMonthAsync(month, year);

            // Lấy danh sách NV active
            var empResult = await _empRepo.GetAllAsync(1, 9999, null, deptId, true);
            var employees = empResult.Items.ToList();

            if (employees.Count == 0)
                return (false, "Không có nhân viên nào để tính lương.", 0);

            int count = 0;
            foreach (var emp in employees)
            {
                // Bỏ qua nếu đã có phiếu Approved
                var existing = await _salaryRepo.GetByEmployeeMonthAsync(emp.EmployeeId, month, year);
                if (existing != null && existing.Status == "Approved") continue;

                // Xóa draft cũ nếu có  
                if (existing != null && existing.Status == "Draft")
                {
                    // Đã xóa ở trên (DeleteDraftByMonthAsync)
                }

                var record = await CalculateForEmployeeAsync(emp, month, year);
                await _salaryRepo.InsertAsync(record);
                count++;
            }

            return (true, $"Đã tính lương cho {count} nhân viên.", count);
        }

        // ===== DUYỆT =====

        public async Task<(bool Ok, string Msg)> ApproveAllAsync(int month, int year, int approvedByUserId)
        {
            await _salaryRepo.BulkUpdateStatusAsync(month, year, "Approved", approvedByUserId);
            return (true, "Đã duyệt tất cả phiếu lương!");
        }

        public async Task<(bool Ok, string Msg)> ApproveAsync(long salaryId, int approvedByUserId)
        {
            var record = await _salaryRepo.GetByIdAsync(salaryId);
            if (record == null) return (false, "Không tìm thấy phiếu lương.");
            if (record.Status == "Approved") return (false, "Phiếu lương đã được duyệt.");

            await _salaryRepo.UpdateStatusAsync(salaryId, "Approved", approvedByUserId);
            return (true, "Duyệt phiếu lương thành công!");
        }

        // ===== QUERY =====

        public Task<IEnumerable<SalaryRecord>> GetRecordsAsync(int month, int year, int? deptId = null)
            => _salaryRepo.GetSalaryRecordsAsync(month, year, deptId);

        public Task<SalaryRecord?> GetByIdAsync(long salaryId)
            => _salaryRepo.GetByIdAsync(salaryId);

        public Task<IEnumerable<SalaryRecord>> GetHistoryAsync(int empId)
            => _salaryRepo.GetHistoryAsync(empId);

        // ===== THUẾ TNCN — Biểu lũy tiến từng phần =====

        /// <summary>
        /// Tính thuế TNCN theo biểu lũy tiến 7 bậc (Việt Nam)
        /// </summary>
        private static decimal CalculatePIT(decimal taxableIncome)
        {
            if (taxableIncome <= 0) return 0;

            // Biểu thuế lũy tiến từng phần (VNĐ/tháng)
            var brackets = new (decimal UpperLimit, decimal Rate)[]
            {
                (5_000_000m,    0.05m),
                (10_000_000m,   0.10m),
                (18_000_000m,   0.15m),
                (32_000_000m,   0.20m),
                (52_000_000m,   0.25m),
                (80_000_000m,   0.30m),
                (decimal.MaxValue, 0.35m)
            };

            decimal tax = 0;
            decimal previousLimit = 0;

            foreach (var (upperLimit, rate) in brackets)
            {
                if (taxableIncome <= previousLimit) break;

                var taxableInBracket = Math.Min(taxableIncome, upperLimit) - previousLimit;
                tax += taxableInBracket * rate;
                previousLimit = upperLimit;
            }

            return Math.Round(tax);
        }
    }
}
