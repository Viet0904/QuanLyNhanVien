-- =============================================
-- Phase 4: Thêm menu Chấm Công (chạy nếu DB đã seed trước đó)
-- =============================================
USE QuanLyNhanVien;
GO

-- Kiểm tra và thêm menu con của "Chấm Công" nếu chưa có
IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = N'CC_NGAY')
BEGIN
    -- Lấy MenuId của nhóm "Chấm Công" (CHAMCONG)
    DECLARE @chamCongId INT;
    SELECT @chamCongId = MenuId FROM Menus WHERE MenuCode = N'CHAMCONG';

    -- Nếu nhóm chưa tồn tại, tạo luôn
    IF @chamCongId IS NULL
    BEGIN
        INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder) 
        VALUES (N'CHAMCONG', N'Chấm Công', NULL, NULL, N'clock', 2);
        SET @chamCongId = SCOPE_IDENTITY();
    END

    -- Thêm 3 menu con
    INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder) VALUES
    (N'CC_NGAY',  N'Chấm Công Ngày',        @chamCongId, N'FrmAttendanceDaily',   N'calendar-check', 1),
    (N'CC_THANG', N'Bảng Chấm Công Tháng',  @chamCongId, N'FrmAttendanceMonthly', N'calendar',       2),
    (N'CC_PHEP',  N'Đơn Xin Nghỉ Phép',     @chamCongId, N'FrmLeaveRequest',      N'document',       3);

    -- Cấp quyền cho Admin (RoleId = 1) trên các menu mới
    INSERT INTO RolePermissions (RoleId, MenuId, CanView, CanAdd, CanEdit, CanDelete, CanExport, CanPrint)
    SELECT 1, MenuId, 1, 1, 1, 1, 1, 1 
    FROM Menus 
    WHERE MenuCode IN (N'CC_NGAY', N'CC_THANG', N'CC_PHEP')
    AND MenuId NOT IN (SELECT MenuId FROM RolePermissions WHERE RoleId = 1);

    PRINT N'✅ Đã thêm menu Chấm Công và phân quyền Admin!';
END
ELSE
    PRINT N'ℹ️ Menu Chấm Công đã tồn tại, bỏ qua.';
GO
