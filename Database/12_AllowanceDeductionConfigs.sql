-- =============================================
-- Seed data: Phụ cấp & Khấu trừ configs
-- Chạy sau 04_SeedData.sql
-- =============================================
USE QuanLyNhanVien;
GO

-- Phụ cấp ăn trưa (VNĐ/tháng)
IF NOT EXISTS (SELECT 1 FROM SalaryConfigs WHERE ConfigCode = 'PC_ANTRUOI')
INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom, IsActive)
VALUES ('PC_ANTRUOI', N'Phụ cấp ăn trưa', 730000, 'Amount', '2024-01-01', 1);
GO

-- Phụ cấp xăng xe (VNĐ/tháng)
IF NOT EXISTS (SELECT 1 FROM SalaryConfigs WHERE ConfigCode = 'PC_XANGXE')
INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom, IsActive)
VALUES ('PC_XANGXE', N'Phụ cấp xăng xe', 500000, 'Amount', '2024-01-01', 1);
GO

-- Phụ cấp đi lại (VNĐ/tháng)
IF NOT EXISTS (SELECT 1 FROM SalaryConfigs WHERE ConfigCode = 'PC_DILAI')
INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom, IsActive)
VALUES ('PC_DILAI', N'Phụ cấp đi lại', 300000, 'Amount', '2024-01-01', 1);
GO

-- Mức phạt đi muộn (VNĐ/lần)
IF NOT EXISTS (SELECT 1 FROM SalaryConfigs WHERE ConfigCode = 'PHAT_DIMUON_MUC')
INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom, IsActive)
VALUES ('PHAT_DIMUON_MUC', N'Mức phạt đi muộn (VNĐ/lần)', 50000, 'Amount', '2024-01-01', 1);
GO

-- Ngưỡng miễn phạt đi muộn (số lần/tháng)
IF NOT EXISTS (SELECT 1 FROM SalaryConfigs WHERE ConfigCode = 'PHAT_DIMUON_NGUONG')
INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom, IsActive)
VALUES ('PHAT_DIMUON_NGUONG', N'Ngưỡng miễn phạt đi muộn (lần/tháng)', 3, 'Count', '2024-01-01', 1);
GO

PRINT N'Seed phụ cấp & khấu trừ thành công!';
GO
