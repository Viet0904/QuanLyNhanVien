-- =============================================
-- Phase 8: Seed data cho Danh muc con thieu
-- =============================================
USE QuanLyNhanVien;
GO

-- Trinh do hoc van
IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryCode = N'EDUCATION_LEVEL')
BEGIN
    INSERT INTO Categories (CategoryCode, CategoryName, [Description])
    VALUES (N'EDUCATION_LEVEL', N'Trình Độ Học Vấn', N'Trình độ học vấn / chuyên môn');

    DECLARE @edCatId INT = SCOPE_IDENTITY();
    INSERT INTO CategoryItems (CategoryId, ItemCode, ItemName, SortOrder) VALUES
    (@edCatId, N'ED_THPT', N'THPT', 1),
    (@edCatId, N'ED_TC', N'Trung cấp', 2),
    (@edCatId, N'ED_CD', N'Cao đẳng', 3),
    (@edCatId, N'ED_DH', N'Đại học', 4),
    (@edCatId, N'ED_THS', N'Thạc sĩ', 5),
    (@edCatId, N'ED_TS', N'Tiến sĩ', 6);
END
GO

-- Loai hop dong
IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryCode = N'CONTRACT_TYPE')
BEGIN
    INSERT INTO Categories (CategoryCode, CategoryName, [Description])
    VALUES (N'CONTRACT_TYPE', N'Loại Hợp Đồng', N'Loại hợp đồng lao động');

    DECLARE @ctCatId INT = SCOPE_IDENTITY();
    INSERT INTO CategoryItems (CategoryId, ItemCode, ItemName, SortOrder) VALUES
    (@ctCatId, N'CT_TV', N'Thử việc', 1),
    (@ctCatId, N'CT_XDTH', N'Xác định thời hạn', 2),
    (@ctCatId, N'CT_KXDTH', N'Không xác định thời hạn', 3),
    (@ctCatId, N'CT_KHOAN', N'Khoán việc', 4);
END
GO

PRINT N'Phase 8 - Category Seed: Done!';
GO
