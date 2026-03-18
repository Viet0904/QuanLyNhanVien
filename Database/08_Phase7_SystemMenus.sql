-- =============================================
-- Phase 7: System Menu - Thêm menu mới + bảng CompanySettings
-- =============================================
USE QuanLyNhanVien;
GO

-- =============================================
-- 1. Bảng CompanySettings - Thiết lập công ty
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CompanySettings')
CREATE TABLE CompanySettings (
    SettingId           INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey           NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue         NVARCHAR(MAX) NULL,
    UpdatedAt            DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- =============================================
-- 2. Thêm menu items mới vào Hệ Thống (ParentId = 5)
-- =============================================

-- Quản lý Người Dùng
IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = N'HT_USER')
INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder, [Description]) VALUES
(N'HT_USER', N'Quản Lý Người Dùng', 5, N'FrmUserManager', N'user', 0, N'Tạo, sửa, vô hiệu hóa tài khoản, gán vai trò');
GO

-- Sao Lưu / Phục Hồi
IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = N'HT_BACKUP')
INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder, [Description]) VALUES
(N'HT_BACKUP', N'Sao Lưu / Phục Hồi', 5, N'FrmBackupRestore', N'settings', 6, N'Backup và Restore database');
GO

-- Thiết Lập Công Ty
IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = N'HT_CAUHINH')
INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder, [Description]) VALUES
(N'HT_CAUHINH', N'Thiết Lập Công Ty', 5, N'FrmCompanySettings', N'settings', 7, N'Cấu hình thông tin công ty, logo');
GO

-- =============================================
-- 3. Gán full quyền Admin cho các menu mới
-- =============================================
INSERT INTO RolePermissions (RoleId, MenuId, CanView, CanAdd, CanEdit, CanDelete, CanExport, CanPrint)
SELECT 1, MenuId, 1, 1, 1, 1, 1, 1 
FROM Menus 
WHERE MenuCode IN (N'HT_USER', N'HT_BACKUP', N'HT_CAUHINH')
AND MenuId NOT IN (SELECT MenuId FROM RolePermissions WHERE RoleId = 1);
GO

-- =============================================
-- 4. Seed data mặc định cho CompanySettings
-- =============================================
IF NOT EXISTS (SELECT 1 FROM CompanySettings WHERE SettingKey = 'CompanyName')
BEGIN
    INSERT INTO CompanySettings (SettingKey, SettingValue) VALUES
    (N'CompanyName', N'Công Ty TNHH ABC'),
    (N'Address', N'123 Đường Nguyễn Văn A, Quận 1, TP.HCM'),
    (N'Phone', N'028-1234-5678'),
    (N'Email', N'info@abc.com.vn'),
    (N'TaxCode', N''),
    (N'Website', N''),
    (N'Fax', N''),
    (N'Representative', N''),
    (N'RepresentativeTitle', N'');
END
GO

PRINT N'Phase 7 - System Menu: Thành công!';
GO
