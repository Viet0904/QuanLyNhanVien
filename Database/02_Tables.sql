-- =============================================
-- Quản Lý Nhân Viên - Tạo bảng
-- =============================================
USE QuanLyNhanVien;
GO

-- =============================================
-- 1. Users - Tài khoản đăng nhập
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
CREATE TABLE Users (
    UserId              INT IDENTITY(1,1) PRIMARY KEY,
    Username            NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash        NVARCHAR(256) NOT NULL,
    Salt                NVARCHAR(128) NOT NULL DEFAULT '',
    IsActive            BIT NOT NULL DEFAULT 1,
    FailedLoginCount    INT NOT NULL DEFAULT 0,
    LockedUntil         DATETIME NULL,
    LastLogin           DATETIME NULL,
    CreatedAt           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt           DATETIME NULL
);
GO

-- =============================================
-- 2. Departments - Phòng ban
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Departments')
CREATE TABLE Departments (
    DepartmentId        INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentCode      NVARCHAR(20) NOT NULL UNIQUE,
    DepartmentName      NVARCHAR(100) NOT NULL,
    ParentId            INT NULL REFERENCES Departments(DepartmentId),
    ManagerId           INT NULL,  -- FK added after Employees table
    IsActive            BIT NOT NULL DEFAULT 1
);
GO

-- =============================================
-- 3. Positions - Chức vụ
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Positions')
CREATE TABLE Positions (
    PositionId          INT IDENTITY(1,1) PRIMARY KEY,
    PositionName        NVARCHAR(100) NOT NULL,
    [Level]             INT NOT NULL DEFAULT 0,
    AllowanceAmount     DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsActive            BIT NOT NULL DEFAULT 1
);
GO

-- =============================================
-- 4. Employees - Nhân viên
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
CREATE TABLE Employees (
    EmployeeId          INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeCode        NVARCHAR(20) NOT NULL UNIQUE,
    UserId              INT NULL REFERENCES Users(UserId),
    FullName            NVARCHAR(100) NOT NULL,
    Gender              NVARCHAR(10) NULL,
    DateOfBirth         DATE NULL,
    IdentityNo          NVARCHAR(20) NULL UNIQUE,
    Phone               NVARCHAR(20) NULL,
    Email               NVARCHAR(100) NULL,
    [Address]           NVARCHAR(500) NULL,
    Photo               VARBINARY(MAX) NULL,
    DepartmentId        INT NOT NULL REFERENCES Departments(DepartmentId),
    PositionId          INT NOT NULL REFERENCES Positions(PositionId),
    HireDate            DATE NOT NULL,
    TerminationDate     DATE NULL,
    BankAccount         NVARCHAR(30) NULL,
    BankName            NVARCHAR(100) NULL,
    TaxCode             NVARCHAR(20) NULL,
    InsuranceNo         NVARCHAR(30) NULL,
    BasicSalary         DECIMAL(18,2) NOT NULL DEFAULT 0,
    SalaryCoefficient   DECIMAL(5,2) NOT NULL DEFAULT 1.0,
    NumberOfDependents  INT NOT NULL DEFAULT 0,
    IsActive            BIT NOT NULL DEFAULT 1,
    Notes               NVARCHAR(MAX) NULL,
    CreatedAt           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt           DATETIME NULL
);
GO

-- Add FK from Departments.ManagerId to Employees
ALTER TABLE Departments ADD CONSTRAINT FK_Dept_Manager
    FOREIGN KEY (ManagerId) REFERENCES Employees(EmployeeId);
GO

-- Indexes
CREATE NONCLUSTERED INDEX IX_Employee_Dept ON Employees(DepartmentId) INCLUDE (FullName, EmployeeCode);
CREATE NONCLUSTERED INDEX IX_Employee_Code ON Employees(EmployeeCode);
CREATE NONCLUSTERED INDEX IX_Employee_Active ON Employees(IsActive) INCLUDE (FullName, DepartmentId);
GO

-- =============================================
-- 5. Menus - Menu động đa cấp
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Menus')
CREATE TABLE Menus (
    MenuId              INT IDENTITY(1,1) PRIMARY KEY,
    MenuCode            NVARCHAR(50) NOT NULL UNIQUE,
    MenuName            NVARCHAR(100) NOT NULL,
    ParentId            INT NULL REFERENCES Menus(MenuId),
    FormName            NVARCHAR(200) NULL,
    IconName            NVARCHAR(100) NULL,
    SortOrder           INT NOT NULL DEFAULT 0,
    IsVisible           BIT NOT NULL DEFAULT 1,
    IsActive            BIT NOT NULL DEFAULT 1,
    [Description]       NVARCHAR(500) NULL
);
GO

-- =============================================
-- 6. Roles - Vai trò
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
CREATE TABLE Roles (
    RoleId              INT IDENTITY(1,1) PRIMARY KEY,
    RoleName            NVARCHAR(100) NOT NULL UNIQUE,
    [Description]       NVARCHAR(500) NULL,
    IsSystem            BIT NOT NULL DEFAULT 0,
    IsActive            BIT NOT NULL DEFAULT 1
);
GO

-- =============================================
-- 7. UserRoles - Gán role cho user
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
CREATE TABLE UserRoles (
    UserRoleId          INT IDENTITY(1,1) PRIMARY KEY,
    UserId              INT NOT NULL REFERENCES Users(UserId),
    RoleId              INT NOT NULL REFERENCES Roles(RoleId),
    AssignedAt          DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_UserRole UNIQUE (UserId, RoleId)
);
GO

-- =============================================
-- 8. RolePermissions - Phân quyền theo menu
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissions')
CREATE TABLE RolePermissions (
    PermissionId        INT IDENTITY(1,1) PRIMARY KEY,
    RoleId              INT NOT NULL REFERENCES Roles(RoleId),
    MenuId              INT NOT NULL REFERENCES Menus(MenuId),
    CanView             BIT NOT NULL DEFAULT 0,
    CanAdd              BIT NOT NULL DEFAULT 0,
    CanEdit             BIT NOT NULL DEFAULT 0,
    CanDelete           BIT NOT NULL DEFAULT 0,
    CanExport           BIT NOT NULL DEFAULT 0,
    CanPrint            BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_RolePerm UNIQUE (RoleId, MenuId)
);
GO

-- =============================================
-- 9. Categories - Danh mục (header)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
CREATE TABLE Categories (
    CategoryId          INT IDENTITY(1,1) PRIMARY KEY,
    CategoryCode        NVARCHAR(50) NOT NULL UNIQUE,
    CategoryName        NVARCHAR(100) NOT NULL,
    [Description]       NVARCHAR(500) NULL,
    IsSystem            BIT NOT NULL DEFAULT 0
);
GO

-- =============================================
-- 10. CategoryItems - Giá trị danh mục
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CategoryItems')
CREATE TABLE CategoryItems (
    ItemId              INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId          INT NOT NULL REFERENCES Categories(CategoryId),
    ItemCode            NVARCHAR(50) NOT NULL,
    ItemName            NVARCHAR(200) NOT NULL,
    ItemValue           NVARCHAR(500) NULL,
    SortOrder           INT NOT NULL DEFAULT 0,
    IsActive            BIT NOT NULL DEFAULT 1
);
GO

-- =============================================
-- 11. AttendanceRecords - Chấm công
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AttendanceRecords')
CREATE TABLE AttendanceRecords (
    RecordId            BIGINT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId          INT NOT NULL REFERENCES Employees(EmployeeId),
    WorkDate            DATE NOT NULL,
    CheckIn             TIME NULL,
    CheckOut            TIME NULL,
    ShiftType           NVARCHAR(20) NULL,
    [Status]            NVARCHAR(20) NOT NULL DEFAULT 'Present',
    OvertimeHours       DECIMAL(5,2) NOT NULL DEFAULT 0,
    Notes               NVARCHAR(500) NULL,
    CreatedBy           INT NOT NULL REFERENCES Users(UserId),
    CreatedAt           DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Attendance UNIQUE (EmployeeId, WorkDate)
);
GO

CREATE NONCLUSTERED INDEX IX_Attendance_Date ON AttendanceRecords(WorkDate) INCLUDE (EmployeeId, [Status]);
GO

-- =============================================
-- 12. LeaveRequests - Nghỉ phép
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LeaveRequests')
CREATE TABLE LeaveRequests (
    LeaveId             INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId          INT NOT NULL REFERENCES Employees(EmployeeId),
    LeaveType           NVARCHAR(30) NOT NULL,
    StartDate           DATE NOT NULL,
    EndDate             DATE NOT NULL,
    TotalDays           DECIMAL(5,1) NOT NULL,
    Reason              NVARCHAR(500) NULL,
    [Status]            NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    ApprovedBy          INT NULL REFERENCES Users(UserId),
    ApprovedAt          DATETIME NULL,
    CreatedAt           DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- =============================================
-- 13. SalaryConfigs - Cấu hình lương
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SalaryConfigs')
CREATE TABLE SalaryConfigs (
    ConfigId            INT IDENTITY(1,1) PRIMARY KEY,
    ConfigCode          NVARCHAR(50) NOT NULL UNIQUE,
    ConfigName          NVARCHAR(100) NOT NULL,
    ConfigValue         DECIMAL(18,4) NOT NULL,
    ConfigType          NVARCHAR(20) NOT NULL DEFAULT 'Percent',
    EffectiveFrom       DATE NOT NULL,
    EffectiveTo         DATE NULL,
    IsActive            BIT NOT NULL DEFAULT 1
);
GO

-- =============================================
-- 14. SalaryRecords - Bảng lương
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SalaryRecords')
CREATE TABLE SalaryRecords (
    SalaryId            BIGINT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId          INT NOT NULL REFERENCES Employees(EmployeeId),
    [Month]             INT NOT NULL,
    [Year]              INT NOT NULL,
    WorkingDays         DECIMAL(5,1) NOT NULL DEFAULT 0,
    StandardDays        DECIMAL(5,1) NOT NULL DEFAULT 26,
    BasicSalary         DECIMAL(18,2) NOT NULL DEFAULT 0,
    SalaryCoefficient   DECIMAL(5,2) NOT NULL DEFAULT 1.0,
    PositionAllowance   DECIMAL(18,2) NOT NULL DEFAULT 0,
    OtherAllowance      DECIMAL(18,2) NOT NULL DEFAULT 0,
    OvertimePay         DECIMAL(18,2) NOT NULL DEFAULT 0,
    GrossIncome         DECIMAL(18,2) NOT NULL DEFAULT 0,
    SocialInsurance     DECIMAL(18,2) NOT NULL DEFAULT 0,
    HealthInsurance     DECIMAL(18,2) NOT NULL DEFAULT 0,
    UnemploymentInsurance DECIMAL(18,2) NOT NULL DEFAULT 0,
    PersonalDeduction   DECIMAL(18,2) NOT NULL DEFAULT 11000000,
    DependentDeduction  DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxableIncome       DECIMAL(18,2) NOT NULL DEFAULT 0,
    PersonalIncomeTax   DECIMAL(18,2) NOT NULL DEFAULT 0,
    OtherDeductions     DECIMAL(18,2) NOT NULL DEFAULT 0,
    NetSalary           DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Status]            NVARCHAR(20) NOT NULL DEFAULT 'Draft',
    ApprovedBy          INT NULL REFERENCES Users(UserId),
    Notes               NVARCHAR(MAX) NULL,
    CreatedAt           DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Salary UNIQUE (EmployeeId, [Month], [Year])
);
GO

-- =============================================
-- 15. AuditLogs - Nhật ký thao tác
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
CREATE TABLE AuditLogs (
    LogId               BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId              INT NOT NULL REFERENCES Users(UserId),
    [Action]            NVARCHAR(50) NOT NULL,
    TableName           NVARCHAR(100) NOT NULL,
    RecordId            NVARCHAR(50) NULL,
    OldValue            NVARCHAR(MAX) NULL,
    NewValue            NVARCHAR(MAX) NULL,
    IPAddress           NVARCHAR(50) NULL,
    CreatedAt           DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE NONCLUSTERED INDEX IX_AuditLog_Date ON AuditLogs(CreatedAt DESC) INCLUDE (UserId, [Action], TableName);
GO

PRINT N'Tạo bảng thành công!';
GO
