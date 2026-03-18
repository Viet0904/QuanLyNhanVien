-- =============================================
-- Fix Vietnamese encoding cho SalaryConfigs
-- Dữ liệu bị mojibake: UTF-8 bytes đọc nhầm thành Latin-1
-- =============================================
USE QuanLyNhanVien;
GO

-- Phụ cấp ăn trưa
UPDATE SalaryConfigs SET ConfigName = N'Phụ cấp ăn trưa' WHERE ConfigCode = 'PC_ANTRUOI';
GO

-- Phụ cấp xăng xe
UPDATE SalaryConfigs SET ConfigName = N'Phụ cấp xăng xe' WHERE ConfigCode = 'PC_XANGXE';
GO

-- Phụ cấp đi lại
UPDATE SalaryConfigs SET ConfigName = N'Phụ cấp đi lại' WHERE ConfigCode = 'PC_DILAI';
GO

-- Mức phạt đi muộn
UPDATE SalaryConfigs SET ConfigName = N'Mức phạt đi muộn (VNĐ/lần)' WHERE ConfigCode = 'PHAT_DIMUON_MUC';
GO

-- Ngưỡng miễn phạt đi muộn
UPDATE SalaryConfigs SET ConfigName = N'Ngưỡng miễn phạt đi muộn (lần/tháng)' WHERE ConfigCode = 'PHAT_DIMUON_NGUONG';
GO

-- Số ngày công chuẩn
UPDATE SalaryConfigs SET ConfigName = N'Số ngày công chuẩn/tháng' WHERE ConfigCode = 'STANDARD_WORK_DAYS';
GO

-- Ngày công
UPDATE SalaryConfigs SET ConfigName = N'Ngày công chuẩn/tháng' WHERE ConfigCode = 'NGAY_CONG';
GO

-- Bảo hiểm thất nghiệp (NLĐ)
UPDATE SalaryConfigs SET ConfigName = N'Bảo hiểm thất nghiệp (NLĐ)' WHERE ConfigCode = 'BHTN';
GO

-- Tỷ lệ BHTN (NV đóng)
UPDATE SalaryConfigs SET ConfigName = N'Tỷ lệ BHTN (NV đóng)' WHERE ConfigCode = 'BHTN_RATE';
GO

-- Bảo hiểm xã hội (NLĐ)
UPDATE SalaryConfigs SET ConfigName = N'Bảo hiểm xã hội (NLĐ)' WHERE ConfigCode = 'BHXH';
GO

-- Tỷ lệ BHXH (NV đóng)
UPDATE SalaryConfigs SET ConfigName = N'Tỷ lệ BHXH (NV đóng)' WHERE ConfigCode = 'BHXH_RATE';
GO

-- Bảo hiểm y tế (NLĐ)
UPDATE SalaryConfigs SET ConfigName = N'Bảo hiểm y tế (NLĐ)' WHERE ConfigCode = 'BHYT';
GO

-- Tỷ lệ BHYT (NV đóng)
UPDATE SalaryConfigs SET ConfigName = N'Tỷ lệ BHYT (NV đóng)' WHERE ConfigCode = 'BHYT_RATE';
GO

-- Giảm trừ người phụ thuộc
UPDATE SalaryConfigs SET ConfigName = N'Giảm trừ người phụ thuộc' WHERE ConfigCode = 'DEPENDENT_DEDUCTION';
GO

-- Giảm trừ bản thân
UPDATE SalaryConfigs SET ConfigName = N'Giảm trừ bản thân' WHERE ConfigCode = 'GIAMTRU_BT';
GO

-- Giảm trừ người phụ thuộc
UPDATE SalaryConfigs SET ConfigName = N'Giảm trừ người phụ thuộc' WHERE ConfigCode = 'GIAMTRU_NPT';
GO

-- Hệ số tăng ca
UPDATE SalaryConfigs SET ConfigName = N'Hệ số tăng ca' WHERE ConfigCode = 'OT_RATE';
GO

-- Hệ số tăng ca ngày lễ
UPDATE SalaryConfigs SET ConfigName = N'Hệ số tăng ca ngày lễ' WHERE ConfigCode = 'OT_RATE_HOLIDAY';
GO

-- Hệ số tăng ca ngày thường
UPDATE SalaryConfigs SET ConfigName = N'Hệ số tăng ca ngày thường' WHERE ConfigCode = 'OT_RATE_WEEKDAY';
GO

-- Hệ số tăng ca cuối tuần
UPDATE SalaryConfigs SET ConfigName = N'Hệ số tăng ca cuối tuần' WHERE ConfigCode = 'OT_RATE_WEEKEND';
GO

-- Giảm trừ cá nhân
UPDATE SalaryConfigs SET ConfigName = N'Giảm trừ cá nhân' WHERE ConfigCode = 'PERSONAL_DEDUCTION';
GO

PRINT N'✅ Fix encoding SalaryConfigs thành công!';
GO
