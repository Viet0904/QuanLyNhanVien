-- =============================================
-- Phase 9: Contracts, EmployeeEvents, Advances + Menus
-- =============================================
USE QuanLyNhanVien;
GO

-- 1. Bang Contracts
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Contracts')
CREATE TABLE Contracts (
    ContractId    INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId    INT NOT NULL REFERENCES Employees(EmployeeId),
    ContractCode  NVARCHAR(50) NOT NULL,
    ContractType  NVARCHAR(20) NOT NULL,
    SignDate      DATE NOT NULL DEFAULT GETDATE(),
    StartDate     DATE NOT NULL DEFAULT GETDATE(),
    EndDate       DATE NULL,
    Notes         NVARCHAR(500) NULL,
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedAt     DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- 2. Bang EmployeeEvents
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmployeeEvents')
CREATE TABLE EmployeeEvents (
    EventId       INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId    INT NOT NULL REFERENCES Employees(EmployeeId),
    EventType     NVARCHAR(20) NOT NULL,
    EventDate     DATE NOT NULL DEFAULT GETDATE(),
    [Description] NVARCHAR(500) NOT NULL,
    Amount        DECIMAL(18,0) NOT NULL DEFAULT 0,
    CreatedBy     NVARCHAR(50) NULL,
    CreatedAt     DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- 3. Bang Advances
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Advances')
CREATE TABLE Advances (
    AdvanceId     INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId    INT NOT NULL REFERENCES Employees(EmployeeId),
    Amount        DECIMAL(18,0) NOT NULL,
    AdvanceDate   DATE NOT NULL DEFAULT GETDATE(),
    [Month]       INT NOT NULL,
    [Year]        INT NOT NULL,
    Status        NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    Notes         NVARCHAR(500) NULL,
    CreatedAt     DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- 4. Menu items moi
-- Hop dong (Menu Nhan su, ParentId = 1)
IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = N'NS_HOPDONG')
INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder, [Description]) VALUES
(N'NS_HOPDONG', N'Hợp Đồng Lao Động', 1, N'FrmContractManager', N'document', 5, N'Quản lý hợp đồng lao động');
GO

-- Bien dong nhan su
IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = N'NS_BIENDONG')
INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder, [Description]) VALUES
(N'NS_BIENDONG', N'Biến Động Nhân Sự', 1, N'FrmEmployeeEvents', N'chart', 6, N'Khen thưởng, kỷ luật, thăng chức, điều chuyển');
GO

-- Tam ung (Menu Tinh luong, ParentId = 4)
IF NOT EXISTS (SELECT 1 FROM Menus WHERE MenuCode = N'TL_TAMUNG')
INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder, [Description]) VALUES
(N'TL_TAMUNG', N'Quản Lý Tạm Ứng', 4, N'FrmAdvanceManager', N'money', 5, N'Tạm ứng lương');
GO

-- 5. Gan quyen Admin
INSERT INTO RolePermissions (RoleId, MenuId, CanView, CanAdd, CanEdit, CanDelete, CanExport, CanPrint)
SELECT 1, MenuId, 1, 1, 1, 1, 1, 1
FROM Menus
WHERE MenuCode IN (N'NS_HOPDONG', N'NS_BIENDONG', N'TL_TAMUNG')
AND MenuId NOT IN (SELECT MenuId FROM RolePermissions WHERE RoleId = 1);
GO

PRINT N'Phase 9 - Contracts/Events/Advances: Done!';
GO
