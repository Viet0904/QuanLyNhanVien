-- =============================================
-- MISS-2, MISS-3 migrations: ContractSalary + more
-- =============================================
USE QuanLyNhanVien;
GO

-- MISS-2: Thêm cột ContractSalary vào Contracts
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Contracts') AND name = 'ContractSalary')
BEGIN
    ALTER TABLE Contracts ADD ContractSalary DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT N'Đã thêm cột ContractSalary vào Contracts.';
END
GO

PRINT N'MISS features migration done!';
GO
