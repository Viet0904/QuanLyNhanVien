-- =============================================
-- Dữ liệu mẫu để test + Đưa Dashboard lên đầu sidebar
-- =============================================
USE QuanLyNhanVien;
GO

-- =============================================
-- 1. ĐƯA DASHBOARD LÊN ĐẦU SIDEBAR
-- Tạo menu Dashboard ở cấp root (không có parent), SortOrder = 0
-- =============================================
-- Xóa Dashboard cũ khỏi nhóm Báo Cáo
IF EXISTS (SELECT 1 FROM Menus WHERE MenuCode = N'BC_DASHBOARD')
    UPDATE Menus SET ParentId = NULL, SortOrder = 0, MenuName = N'Dashboard', IconName = N'dashboard'
    WHERE MenuCode = N'BC_DASHBOARD';
GO

-- Cập nhật SortOrder các nhóm root để Dashboard đứng đầu
UPDATE Menus SET SortOrder = SortOrder + 1 WHERE ParentId IS NULL AND MenuCode != N'BC_DASHBOARD';
GO

-- Gán lại quyền admin cho Dashboard
IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = 1 AND MenuId = (SELECT MenuId FROM Menus WHERE MenuCode = N'BC_DASHBOARD'))
    INSERT INTO RolePermissions (RoleId, MenuId, CanView, CanAdd, CanEdit, CanDelete, CanExport, CanPrint)
    SELECT 1, MenuId, 1, 1, 1, 1, 1, 1 FROM Menus WHERE MenuCode = N'BC_DASHBOARD';
GO

-- =============================================
-- 2. NHÂN VIÊN MẪU (20 người)
-- =============================================
-- Xóa dữ liệu cũ nếu có (để chạy lại được)
DELETE FROM Advances WHERE EmployeeId IN (SELECT EmployeeId FROM Employees WHERE EmployeeCode LIKE 'NV%');
DELETE FROM SalaryRecords WHERE EmployeeId IN (SELECT EmployeeId FROM Employees WHERE EmployeeCode LIKE 'NV%');
DELETE FROM AttendanceRecords WHERE EmployeeId IN (SELECT EmployeeId FROM Employees WHERE EmployeeCode LIKE 'NV%');
DELETE FROM LeaveRequests WHERE EmployeeId IN (SELECT EmployeeId FROM Employees WHERE EmployeeCode LIKE 'NV%');
DELETE FROM Contracts WHERE EmployeeId IN (SELECT EmployeeId FROM Employees WHERE EmployeeCode LIKE 'NV%');
DELETE FROM EmployeeEvents WHERE EmployeeId IN (SELECT EmployeeId FROM Employees WHERE EmployeeCode LIKE 'NV%');
DELETE FROM Employees WHERE EmployeeCode LIKE 'NV%';
GO

INSERT INTO Employees (EmployeeCode, FullName, Gender, DateOfBirth, IdentityNo, Phone, Email, [Address], DepartmentId, PositionId, HireDate, BankAccount, BankName, TaxCode, InsuranceNo, BasicSalary, SalaryCoefficient, NumberOfDependents, Notes) VALUES
(N'NV001', N'Nguyễn Văn An',      N'Nam',  '1985-03-15', N'012345678901', N'0901234001', N'an.nguyen@company.vn',     N'123 Nguyễn Huệ, Q.1, TP.HCM',         1, 1, '2018-01-15', N'0123456789', N'Vietcombank',  N'MST001', N'BH001', 30000000, 3.50, 2, N'Giám đốc công ty'),
(N'NV002', N'Trần Thị Bình',      N'Nữ',   '1988-07-22', N'012345678902', N'0901234002', N'binh.tran@company.vn',     N'456 Lê Lợi, Q.3, TP.HCM',             1, 2, '2019-03-01', N'0987654321', N'Techcombank',  N'MST002', N'BH002', 25000000, 3.00, 1, N'Phó giám đốc'),
(N'NV003', N'Lê Hoàng Cường',     N'Nam',  '1990-11-08', N'012345678903', N'0901234003', N'cuong.le@company.vn',      N'789 Trần Hưng Đạo, Q.5, TP.HCM',      2, 3, '2019-06-15', N'1122334455', N'MB Bank',      N'MST003', N'BH003', 20000000, 2.50, 1, N'Trưởng phòng nhân sự'),
(N'NV004', N'Phạm Thị Dung',      N'Nữ',   '1992-04-30', N'012345678904', N'0901234004', N'dung.pham@company.vn',     N'321 Võ Văn Tần, Q.3, TP.HCM',          3, 3, '2020-01-10', N'2233445566', N'BIDV',         N'MST004', N'BH004', 20000000, 2.50, 0, N'Trưởng phòng kế toán'),
(N'NV005', N'Hoàng Văn Em',       N'Nam',  '1991-09-12', N'012345678905', N'0901234005', N'em.hoang@company.vn',      N'654 Nguyễn Thị Minh Khai, Q.1, TP.HCM',4, 3, '2019-08-20', N'3344556677', N'VietinBank',   N'MST005', N'BH005', 22000000, 2.80, 2, N'Trưởng phòng IT'),
(N'NV006', N'Võ Thị Phương',      N'Nữ',   '1993-01-25', N'012345678906', N'0901234006', N'phuong.vo@company.vn',     N'987 Pasteur, Q.3, TP.HCM',             5, 4, '2020-04-15', N'4455667788', N'ACB',          N'MST006', N'BH006', 18000000, 2.20, 0, N'Phó phòng kinh doanh'),
(N'NV007', N'Đặng Minh Giang',    N'Nam',  '1994-06-18', N'012345678907', N'0901234007', N'giang.dang@company.vn',    N'147 Hai Bà Trưng, Q.1, TP.HCM',        6, 5, '2021-02-01', N'5566778899', N'TPBank',       N'MST007', N'BH007', 16000000, 2.00, 1, N'Nhóm trưởng marketing'),
(N'NV008', N'Bùi Thị Hạnh',       N'Nữ',   '1995-12-05', N'012345678908', N'0901234008', N'hanh.bui@company.vn',      N'258 CMT8, Q.10, TP.HCM',               2, 6, '2021-05-10', N'6677889900', N'SHB',          N'MST008', N'BH008', 14000000, 1.80, 0, N'Chuyên viên tuyển dụng'),
(N'NV009', N'Ngô Thanh Inh',      N'Nam',  '1996-02-28', N'012345678909', N'0901234009', N'inh.ngo@company.vn',       N'369 Điện Biên Phủ, Bình Thạnh, TP.HCM',4, 6, '2021-08-01', N'7788990011', N'Vietcombank',  N'MST009', N'BH009', 15000000, 1.80, 0, N'Developer'),
(N'NV010', N'Lý Thị Kim',         N'Nữ',   '1997-08-14', N'012345678910', N'0901234010', N'kim.ly@company.vn',        N'741 Cách Mạng T8, Q.3, TP.HCM',        3, 7, '2022-01-05', N'8899001122', N'MB Bank',      N'MST010', N'BH010', 12000000, 1.50, 0, N'Nhân viên kế toán'),
(N'NV011', N'Trương Văn Long',    N'Nam',  '1993-05-20', N'012345678911', N'0901234011', N'long.truong@company.vn',   N'852 Lê Văn Sỹ, Q.3, TP.HCM',           5, 7, '2022-03-15', N'9900112233', N'Techcombank',  N'MST011', N'BH011', 12000000, 1.50, 1, N'Nhân viên kinh doanh'),
(N'NV012', N'Mai Thị Ngọc',       N'Nữ',   '1998-10-30', N'012345678912', N'0901234012', N'ngoc.mai@company.vn',      N'963 Nguyễn Đình Chiểu, Q.3, TP.HCM',   6, 7, '2022-06-01', N'1100223344', N'BIDV',         N'MST012', N'BH012', 11000000, 1.30, 0, N'Nhân viên content'),
(N'NV013', N'Phan Quốc Oanh',     N'Nam',  '1995-07-07', N'012345678913', N'0901234013', N'oanh.phan@company.vn',     N'174 Bùi Viện, Q.1, TP.HCM',             4, 6, '2021-10-01', N'2211334455', N'VietinBank',   N'MST013', N'BH013', 16000000, 1.90, 0, N'Senior Developer'),
(N'NV014', N'Đỗ Thị Phượng',      N'Nữ',   '1999-03-18', N'012345678914', N'0901234014', N'phuong.do@company.vn',     N'285 Nguyễn Trãi, Q.5, TP.HCM',          7, 7, '2023-01-10', N'3322445566', N'ACB',          N'MST014', N'BH014', 10000000, 1.20, 0, N'Nhân viên hành chính'),
(N'NV015', N'Huỳnh Văn Quang',    N'Nam',  '1994-11-11', N'012345678915', N'0901234015', N'quang.huynh@company.vn',   N'396 Phan Xích Long, Phú Nhuận, TP.HCM', 5, 6, '2021-04-01', N'4433556677', N'TPBank',       N'MST015', N'BH015', 15000000, 1.80, 1, N'Chuyên viên bán hàng'),
(N'NV016', N'Lâm Thị Ry',         N'Nữ',   '2000-06-25', N'012345678916', N'0901234016', N'ry.lam@company.vn',        N'507 Lý Thường Kiệt, Q.10, TP.HCM',      4, 7, '2023-06-15', N'5544667788', N'SHB',          N'MST016', N'BH016',  9000000, 1.10, 0, N'Junior Developer'),
(N'NV017', N'Cao Văn Sơn',        N'Nam',  '1991-08-03', N'012345678917', N'0901234017', N'son.cao@company.vn',       N'618 Ba Tháng Hai, Q.10, TP.HCM',        7, 4, '2020-07-01', N'6655778899', N'Vietcombank',  N'MST017', N'BH017', 17000000, 2.10, 2, N'Phó phòng hành chính'),
(N'NV018', N'Đinh Thị Thúy',      N'Nữ',   '1997-01-09', N'012345678918', N'0901234018', N'thuy.dinh@company.vn',     N'729 Hoàng Sa, Q.1, TP.HCM',              2, 7, '2022-09-01', N'7766889900', N'MB Bank',      N'MST018', N'BH018', 11000000, 1.40, 0, N'Nhân viên C&B'),
(N'NV019', N'Tống Văn Uy',        N'Nam',  '1996-04-14', N'012345678919', N'0901234019', N'uy.tong@company.vn',       N'830 Trường Sa, Q.3, TP.HCM',             4, 7, '2023-03-01', N'8877990011', N'Techcombank',  N'MST019', N'BH019', 10000000, 1.20, 0, N'Nhân viên QA'),
(N'NV020', N'Vương Thị Xuân',     N'Nữ',   '2001-12-20', N'012345678920', N'0901234020', N'xuan.vuong@company.vn',    N'941 Nguyễn Kiệm, Gò Vấp, TP.HCM',       6, 8, '2024-01-15', N'9988001122', N'BIDV',         N'MST020', N'BH020',  5000000, 1.00, 0, N'Thực tập marketing');
GO

PRINT N'Đã thêm 20 nhân viên mẫu!';
GO

-- =============================================
-- 3. HỢP ĐỒNG MẪU
-- =============================================
DECLARE @i INT = 1;
WHILE @i <= 20
BEGIN
    DECLARE @empId INT = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = CONCAT('NV', RIGHT('000' + CAST(@i AS VARCHAR), 3)));
    DECLARE @hireDate DATE = (SELECT HireDate FROM Employees WHERE EmployeeId = @empId);
    
    IF @empId IS NOT NULL
    BEGIN
        INSERT INTO Contracts (EmployeeId, ContractCode, ContractType, SignDate, StartDate, EndDate, Notes, IsActive)
        VALUES (@empId, 
                CONCAT('HD-', RIGHT('000' + CAST(@i AS VARCHAR), 3)),
                CASE WHEN @i <= 5 THEN N'Không thời hạn' WHEN @i <= 15 THEN N'Có thời hạn' ELSE N'Thử việc' END,
                @hireDate, @hireDate,
                CASE WHEN @i <= 5 THEN NULL ELSE DATEADD(YEAR, CASE WHEN @i <= 15 THEN 2 ELSE 0 END, DATEADD(MONTH, CASE WHEN @i > 15 THEN 2 ELSE 0 END, @hireDate)) END,
                N'Hợp đồng lao động',
                1);
    END
    SET @i = @i + 1;
END
GO

PRINT N'Đã thêm hợp đồng mẫu!';
GO

-- =============================================
-- 4. CHẤM CÔNG THÁNG 3/2026 (20 ngày làm việc)
-- =============================================
DECLARE @d DATE = '2026-03-02'; -- Bắt đầu tháng 3
WHILE @d <= '2026-03-18'
BEGIN
    IF DATEPART(WEEKDAY, @d) NOT IN (1, 7) -- Không phải CN, T7
    BEGIN
        INSERT INTO AttendanceRecords (EmployeeId, WorkDate, CheckIn, CheckOut, ShiftType, [Status], OvertimeHours, CreatedBy)
        SELECT e.EmployeeId, @d,
               DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 30 - 15, '07:30'), -- check in ~7:15-7:45
               DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 30, '17:00'),     -- check out ~17:00-17:30
               N'FULLDAY', 
               CASE 
                   WHEN ABS(CHECKSUM(NEWID())) % 20 = 0 THEN N'Late'
                   WHEN ABS(CHECKSUM(NEWID())) % 25 = 0 THEN N'Absent'
                   ELSE N'Present'
               END,
               CASE WHEN ABS(CHECKSUM(NEWID())) % 5 = 0 THEN CAST(ABS(CHECKSUM(NEWID())) % 3 + 1 AS DECIMAL(5,2)) ELSE 0 END,
               1
        FROM Employees e
        WHERE e.EmployeeCode LIKE 'NV%'
        AND NOT EXISTS (SELECT 1 FROM AttendanceRecords ar WHERE ar.EmployeeId = e.EmployeeId AND ar.WorkDate = @d);
    END
    SET @d = DATEADD(DAY, 1, @d);
END
GO

PRINT N'Đã thêm chấm công tháng 3/2026!';
GO

-- =============================================
-- 5. ĐƠN NGHỈ PHÉP MẪU (5 đơn)
-- =============================================
INSERT INTO LeaveRequests (EmployeeId, LeaveType, StartDate, EndDate, TotalDays, Reason, [Status], ApprovedBy)
SELECT TOP 1 EmployeeId, N'ANNUAL', '2026-03-20', '2026-03-21', 2, N'Việc gia đình', N'Approved', 1
FROM Employees WHERE EmployeeCode = 'NV008'
AND NOT EXISTS (SELECT 1 FROM LeaveRequests lr WHERE lr.EmployeeId = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'NV008') AND lr.StartDate = '2026-03-20');

INSERT INTO LeaveRequests (EmployeeId, LeaveType, StartDate, EndDate, TotalDays, Reason, [Status])
SELECT TOP 1 EmployeeId, N'SICK', '2026-03-15', '2026-03-15', 1, N'Ốm', N'Approved'
FROM Employees WHERE EmployeeCode = 'NV012'
AND NOT EXISTS (SELECT 1 FROM LeaveRequests lr WHERE lr.EmployeeId = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'NV012') AND lr.StartDate = '2026-03-15');

INSERT INTO LeaveRequests (EmployeeId, LeaveType, StartDate, EndDate, TotalDays, Reason, [Status])
SELECT TOP 1 EmployeeId, N'ANNUAL', '2026-03-25', '2026-03-28', 4, N'Du lịch', N'Pending'
FROM Employees WHERE EmployeeCode = 'NV005'
AND NOT EXISTS (SELECT 1 FROM LeaveRequests lr WHERE lr.EmployeeId = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'NV005') AND lr.StartDate = '2026-03-25');

INSERT INTO LeaveRequests (EmployeeId, LeaveType, StartDate, EndDate, TotalDays, Reason, [Status])
SELECT TOP 1 EmployeeId, N'UNPAID', '2026-04-01', '2026-04-02', 2, N'Việc cá nhân', N'Pending'
FROM Employees WHERE EmployeeCode = 'NV015'
AND NOT EXISTS (SELECT 1 FROM LeaveRequests lr WHERE lr.EmployeeId = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'NV015') AND lr.StartDate = '2026-04-01');

INSERT INTO LeaveRequests (EmployeeId, LeaveType, StartDate, EndDate, TotalDays, Reason, [Status])
SELECT TOP 1 EmployeeId, N'SICK', '2026-03-10', '2026-03-12', 3, N'Bệnh viện', N'Rejected'
FROM Employees WHERE EmployeeCode = 'NV019'
AND NOT EXISTS (SELECT 1 FROM LeaveRequests lr WHERE lr.EmployeeId = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'NV019') AND lr.StartDate = '2026-03-10');
GO

PRINT N'Đã thêm đơn nghỉ phép mẫu!';
GO

-- =============================================
-- 6. LƯƠNG THÁNG 2/2026 (tính sẵn)
-- =============================================
INSERT INTO SalaryRecords (EmployeeId, [Month], [Year], WorkingDays, StandardDays, BasicSalary, SalaryCoefficient, PositionAllowance, OtherAllowance, OvertimePay, GrossIncome, SocialInsurance, HealthInsurance, UnemploymentInsurance, PersonalDeduction, DependentDeduction, TaxableIncome, PersonalIncomeTax, NetSalary, [Status])
SELECT e.EmployeeId, 2, 2026, 
       22, -- ngày công
       26, -- chuẩn
       e.BasicSalary,
       e.SalaryCoefficient,
       p.AllowanceAmount,
       0, -- other allowance
       CASE WHEN e.BasicSalary >= 15000000 THEN 2000000 ELSE 0 END, -- overtime
       (e.BasicSalary * e.SalaryCoefficient * 22 / 26) + p.AllowanceAmount + CASE WHEN e.BasicSalary >= 15000000 THEN 2000000 ELSE 0 END, -- gross
       e.BasicSalary * 0.08, -- BHXH
       e.BasicSalary * 0.015, -- BHYT
       e.BasicSalary * 0.01, -- BHTN
       11000000, -- giảm trừ bản thân
       e.NumberOfDependents * 4400000, -- giảm trừ phụ thuộc
       CASE WHEN ((e.BasicSalary * e.SalaryCoefficient * 22 / 26) + p.AllowanceAmount - e.BasicSalary * 0.105 - 11000000 - e.NumberOfDependents * 4400000) > 0 
            THEN (e.BasicSalary * e.SalaryCoefficient * 22 / 26) + p.AllowanceAmount - e.BasicSalary * 0.105 - 11000000 - e.NumberOfDependents * 4400000
            ELSE 0 END, -- taxable
       0, -- PIT (simplified)
       (e.BasicSalary * e.SalaryCoefficient * 22 / 26) + p.AllowanceAmount - e.BasicSalary * 0.105, -- net (simplified)
       N'Approved'
FROM Employees e
JOIN Positions p ON e.PositionId = p.PositionId
WHERE e.EmployeeCode LIKE 'NV%'
AND NOT EXISTS (SELECT 1 FROM SalaryRecords sr WHERE sr.EmployeeId = e.EmployeeId AND sr.[Month] = 2 AND sr.[Year] = 2026);
GO

PRINT N'Đã thêm bảng lương tháng 2/2026!';
GO

-- =============================================
-- 7. SỰ KIỆN NHÂN SỰ MẪU
-- =============================================
INSERT INTO EmployeeEvents (EmployeeId, EventType, EventDate, [Description], Amount, CreatedBy)
SELECT TOP 1 EmployeeId, N'Reward', '2026-02-01', N'Khen thưởng nhân viên xuất sắc Q4/2025', 5000000, N'admin'
FROM Employees WHERE EmployeeCode = 'NV009'
AND NOT EXISTS (SELECT 1 FROM EmployeeEvents WHERE EmployeeId = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'NV009') AND EventType = 'Reward');

INSERT INTO EmployeeEvents (EmployeeId, EventType, EventDate, [Description], Amount, CreatedBy)
SELECT TOP 1 EmployeeId, N'Promotion', '2026-01-15', N'Thăng chức từ Nhân viên lên Chuyên viên', 0, N'admin'
FROM Employees WHERE EmployeeCode = 'NV011'
AND NOT EXISTS (SELECT 1 FROM EmployeeEvents WHERE EmployeeId = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'NV011') AND EventType = 'Promotion');

INSERT INTO EmployeeEvents (EmployeeId, EventType, EventDate, [Description], Amount, CreatedBy)
SELECT TOP 1 EmployeeId, N'Discipline', '2026-02-15', N'Cảnh cáo đi muộn nhiều lần', 0, N'admin'
FROM Employees WHERE EmployeeCode = 'NV016'
AND NOT EXISTS (SELECT 1 FROM EmployeeEvents WHERE EmployeeId = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'NV016') AND EventType = 'Discipline');

INSERT INTO EmployeeEvents (EmployeeId, EventType, EventDate, [Description], Amount, CreatedBy)
SELECT TOP 1 EmployeeId, N'Transfer', '2026-03-01', N'Điều chuyển từ phòng Marketing sang phòng Kinh doanh', 0, N'admin'
FROM Employees WHERE EmployeeCode = 'NV012'
AND NOT EXISTS (SELECT 1 FROM EmployeeEvents WHERE EmployeeId = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'NV012') AND EventType = 'Transfer');
GO

PRINT N'Đã thêm sự kiện nhân sự mẫu!';
GO

-- =============================================
-- 8. TẠM ỨNG MẪU
-- =============================================
INSERT INTO Advances (EmployeeId, Amount, AdvanceDate, [Month], [Year], [Status], Notes)
SELECT TOP 1 EmployeeId, 5000000, '2026-03-05', 3, 2026, N'Approved', N'Tạm ứng lương tháng 3'
FROM Employees WHERE EmployeeCode = 'NV010'
AND NOT EXISTS (SELECT 1 FROM Advances WHERE EmployeeId = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'NV010') AND [Month] = 3 AND [Year] = 2026);

INSERT INTO Advances (EmployeeId, Amount, AdvanceDate, [Month], [Year], [Status], Notes)
SELECT TOP 1 EmployeeId, 3000000, '2026-03-10', 3, 2026, N'Pending', N'Tạm ứng khẩn cấp'
FROM Employees WHERE EmployeeCode = 'NV014'
AND NOT EXISTS (SELECT 1 FROM Advances WHERE EmployeeId = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'NV014') AND [Month] = 3 AND [Year] = 2026);

INSERT INTO Advances (EmployeeId, Amount, AdvanceDate, [Month], [Year], [Status], Notes)
SELECT TOP 1 EmployeeId, 8000000, '2026-02-20', 2, 2026, N'Approved', N'Tạm ứng lương tháng 2'
FROM Employees WHERE EmployeeCode = 'NV007'
AND NOT EXISTS (SELECT 1 FROM Advances WHERE EmployeeId = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'NV007') AND [Month] = 2 AND [Year] = 2026);
GO

PRINT N'Đã thêm tạm ứng mẫu!';
GO

PRINT N'====================================';
PRINT N'DỮ LIỆU MẪU ĐÃ THÊM THÀNH CÔNG!';
PRINT N'- Dashboard đã lên đầu sidebar';
PRINT N'- 20 nhân viên mẫu';
PRINT N'- Hợp đồng cho 20 NV';
PRINT N'- Chấm công tháng 3/2026';
PRINT N'- 5 đơn nghỉ phép';
PRINT N'- Bảng lương tháng 2/2026';
PRINT N'- 4 sự kiện nhân sự';
PRINT N'- 3 tạm ứng lương';
PRINT N'====================================';
GO
