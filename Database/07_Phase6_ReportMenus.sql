-- =============================================
-- Phase 6: Report & Dashboard Menus
-- Chạy trên database QuanLyNhanVien
-- An toàn để chạy nhiều lần (idempotent)
-- =============================================

USE QuanLyNhanVien;
GO

-- 1. Tạo menu group "Báo Cáo"
DECLARE @parentId INT = NULL;
IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = 'BAOCAO')
BEGIN
    INSERT INTO Menus (MenuCode, MenuName, ParentId, SortOrder, IconName, FormName, IsVisible, IsActive)
    VALUES ('BAOCAO', N'Báo Cáo', NULL, 60, 'chart', NULL, 1, 1);
END
SELECT @parentId = MenuId FROM Menus WHERE MenuCode = 'BAOCAO';

-- 2. Sub-menus
IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = 'BC_DB')
    INSERT INTO Menus (MenuCode, MenuName, ParentId, SortOrder, IconName, FormName, IsVisible, IsActive)
    VALUES ('BC_DB', N'Dashboard', @parentId, 1, 'dashboard', 'FrmDashboard', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = 'BC_RP')
    INSERT INTO Menus (MenuCode, MenuName, ParentId, SortOrder, IconName, FormName, IsVisible, IsActive)
    VALUES ('BC_RP', N'Xuất Báo Cáo', @parentId, 2, 'export', 'FrmReportExport', 1, 1);

-- 3. Grant permissions cho Admin role
DECLARE @adminRoleId INT;
SELECT @adminRoleId = RoleId FROM Roles WHERE RoleName = 'Administrator';

IF @adminRoleId IS NOT NULL
BEGIN
    INSERT INTO RolePermissions (RoleId, MenuId, CanView, CanAdd, CanEdit, CanDelete, CanExport, CanPrint)
    SELECT @adminRoleId, m.MenuId, 1, 1, 1, 1, 1, 1
    FROM Menus m
    WHERE m.MenuCode IN ('BAOCAO', 'BC_DB', 'BC_RP')
      AND NOT EXISTS (SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = @adminRoleId AND rp.MenuId = m.MenuId);
END

PRINT N'Phase 6: Report & Dashboard menus seeded successfully!';
GO
