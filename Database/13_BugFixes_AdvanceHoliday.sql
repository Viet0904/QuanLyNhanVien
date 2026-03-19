-- =============================================
-- Bug Fixes: AdvanceAmount column + Holidays table
-- =============================================
USE QuanLyNhanVien;
GO

-- BUG-1: Thêm cột AdvanceAmount vào SalaryRecords
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SalaryRecords') AND name = 'AdvanceAmount')
BEGIN
    ALTER TABLE SalaryRecords ADD AdvanceAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT N'Đã thêm cột AdvanceAmount vào SalaryRecords.';
END
GO

-- BUG-2: Bảng Holidays — quản lý ngày lễ thay vì hardcode
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Holidays')
CREATE TABLE Holidays (
    HolidayId       INT IDENTITY(1,1) PRIMARY KEY,
    HolidayName     NVARCHAR(200) NOT NULL,
    HolidayDate     DATE NOT NULL,
    [Year]          INT NOT NULL,
    IsRecurring     BIT NOT NULL DEFAULT 0,  -- 1 = lặp hàng năm (dương lịch)
    IsActive        BIT NOT NULL DEFAULT 1,
    Notes           NVARCHAR(500) NULL,
    CONSTRAINT UQ_Holiday UNIQUE (HolidayDate)
);
GO

-- Seed ngày lễ 2026
INSERT INTO Holidays (HolidayName, HolidayDate, [Year], IsRecurring, Notes) VALUES
-- Ngày lễ dương lịch cố định (IsRecurring = 1)
(N'Tết Dương lịch',                 '2026-01-01', 2026, 1, N'Nghỉ 1 ngày'),
(N'Ngày Giải phóng miền Nam',       '2026-04-30', 2026, 1, N'30/4'),
(N'Ngày Quốc tế Lao động',         '2026-05-01', 2026, 1, N'1/5'),
(N'Ngày Quốc khánh',               '2026-09-02', 2026, 1, N'2/9'),
(N'Nghỉ bù Quốc khánh',            '2026-09-03', 2026, 1, N'3/9'),

-- Ngày lễ âm lịch (thay đổi mỗi năm, IsRecurring = 0)
(N'Tết Nguyên Đán - 29 Tết',       '2026-02-16', 2026, 0, N'29 tháng Chạp'),
(N'Tết Nguyên Đán - 30 Tết',       '2026-02-17', 2026, 0, N'30 tháng Chạp'),
(N'Tết Nguyên Đán - Mùng 1',       '2026-02-17', 2026, 0, N'Mùng 1 Tết'),
(N'Tết Nguyên Đán - Mùng 2',       '2026-02-18', 2026, 0, N'Mùng 2 Tết'),
(N'Tết Nguyên Đán - Mùng 3',       '2026-02-19', 2026, 0, N'Mùng 3 Tết'),
(N'Tết Nguyên Đán - Mùng 4',       '2026-02-20', 2026, 0, N'Mùng 4 Tết'),
(N'Tết Nguyên Đán - Mùng 5',       '2026-02-21', 2026, 0, N'Mùng 5 Tết'),
(N'Giỗ Tổ Hùng Vương',             '2026-04-12', 2026, 0, N'10/3 âm lịch');
GO

-- Menu cho quản lý ngày lễ (trong nhóm Danh mục, ParentId = 2)
IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = N'DM_NGAYLE')
INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder, [Description]) VALUES
(N'DM_NGAYLE', N'Quản Lý Ngày Lễ', 2, N'FrmHolidayManager', N'calendar', 7, N'Quản lý ngày lễ, ngày nghỉ');
GO

-- Gán quyền Admin cho menu mới
INSERT INTO RolePermissions (RoleId, MenuId, CanView, CanAdd, CanEdit, CanDelete, CanExport, CanPrint)
SELECT 1, MenuId, 1, 1, 1, 1, 1, 1
FROM Menus WHERE MenuCode = N'DM_NGAYLE'
AND MenuId NOT IN (SELECT MenuId FROM RolePermissions WHERE RoleId = 1);
GO

PRINT N'Bug fixes migration done!';
GO
