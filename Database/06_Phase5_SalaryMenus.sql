-- =============================================
-- Phase 5: Salary Menus + Default Configs
-- Chạy trên database QuanLyNhanVien
-- An toàn để chạy nhiều lần (idempotent)
-- =============================================

USE QuanLyNhanVien;
GO

-- 1. Tạo menu group "Lương"
DECLARE @parentId INT = NULL;
IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = 'LUONG')
BEGIN
    INSERT INTO Menus (MenuCode, MenuName, ParentId, SortOrder, IconName, FormName, IsVisible, IsActive)
    VALUES ('LUONG', N'Lương', NULL, 50, 'money', NULL, 1, 1);
END
SELECT @parentId = MenuId FROM Menus WHERE MenuCode = 'LUONG';

-- 2. Sub-menus
IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = 'L_CH')
    INSERT INTO Menus (MenuCode, MenuName, ParentId, SortOrder, IconName, FormName, IsVisible, IsActive)
    VALUES ('L_CH', N'Cấu Hình Lương', @parentId, 1, 'settings', 'FrmSalaryConfig', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = 'L_TL')
    INSERT INTO Menus (MenuCode, MenuName, ParentId, SortOrder, IconName, FormName, IsVisible, IsActive)
    VALUES ('L_TL', N'Tính Lương Tháng', @parentId, 2, 'calculator', 'FrmSalaryCalculation', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = 'L_PL')
    INSERT INTO Menus (MenuCode, MenuName, ParentId, SortOrder, IconName, FormName, IsVisible, IsActive)
    VALUES ('L_PL', N'Phiếu Lương', @parentId, 3, 'receipt', 'FrmPayslip', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = 'L_LS')
    INSERT INTO Menus (MenuCode, MenuName, ParentId, SortOrder, IconName, FormName, IsVisible, IsActive)
    VALUES ('L_LS', N'Lịch Sử Lương', @parentId, 4, 'history', 'FrmSalaryHistory', 1, 1);

-- 3. Grant permissions cho Admin role
DECLARE @adminRoleId INT;
SELECT @adminRoleId = RoleId FROM Roles WHERE RoleName = 'Administrator';

IF @adminRoleId IS NOT NULL
BEGIN
    INSERT INTO RolePermissions (RoleId, MenuId, CanView, CanAdd, CanEdit, CanDelete, CanExport, CanPrint)
    SELECT @adminRoleId, m.MenuId, 1, 1, 1, 1, 1, 1
    FROM Menus m
    WHERE m.MenuCode IN ('LUONG', 'L_CH', 'L_TL', 'L_PL', 'L_LS')
      AND NOT EXISTS (SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = @adminRoleId AND rp.MenuId = m.MenuId);
END

-- 4. Seed SalaryConfigs mặc định
IF NOT EXISTS (SELECT 1 FROM SalaryConfigs WHERE ConfigCode = 'BHXH')
    INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom)
    VALUES ('BHXH', N'Bảo hiểm xã hội (NLĐ)', 8.0, 'Percent', '2024-01-01');

IF NOT EXISTS (SELECT 1 FROM SalaryConfigs WHERE ConfigCode = 'BHYT')
    INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom)
    VALUES ('BHYT', N'Bảo hiểm y tế (NLĐ)', 1.5, 'Percent', '2024-01-01');

IF NOT EXISTS (SELECT 1 FROM SalaryConfigs WHERE ConfigCode = 'BHTN')
    INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom)
    VALUES ('BHTN', N'Bảo hiểm thất nghiệp (NLĐ)', 1.0, 'Percent', '2024-01-01');

IF NOT EXISTS (SELECT 1 FROM SalaryConfigs WHERE ConfigCode = 'GIAMTRU_BT')
    INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom)
    VALUES ('GIAMTRU_BT', N'Giảm trừ bản thân', 11000000, 'Amount', '2024-01-01');

IF NOT EXISTS (SELECT 1 FROM SalaryConfigs WHERE ConfigCode = 'GIAMTRU_NPT')
    INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom)
    VALUES ('GIAMTRU_NPT', N'Giảm trừ người phụ thuộc', 4400000, 'Amount', '2024-01-01');

IF NOT EXISTS (SELECT 1 FROM SalaryConfigs WHERE ConfigCode = 'NGAY_CONG')
    INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom)
    VALUES ('NGAY_CONG', N'Ngày công chuẩn/tháng', 26, 'Days', '2024-01-01');

IF NOT EXISTS (SELECT 1 FROM SalaryConfigs WHERE ConfigCode = 'OT_RATE')
    INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom)
    VALUES ('OT_RATE', N'Hệ số tăng ca', 1.5, 'Multiplier', '2024-01-01');

PRINT N'Phase 5: Salary menus & configs seeded successfully!';
GO
