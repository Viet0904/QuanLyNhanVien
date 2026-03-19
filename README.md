# 📋 Quản Lý Nhân Viên — Employee Management System

> Ứng dụng quản lý nhân sự desktop xây dựng trên **.NET 8 WinForms**, **SQL Server**, **Dapper** micro-ORM, kiến trúc **3 lớp** (UI → BLL → DAL).

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![WinForms](https://img.shields.io/badge/UI-WinForms-0078D4?logo=windows)
![SQL Server](https://img.shields.io/badge/DB-SQL%20Server-CC2927?logo=microsoftsqlserver)
![Dapper](https://img.shields.io/badge/ORM-Dapper-FF6F00)
![xUnit](https://img.shields.io/badge/Tests-xUnit-512BD4)
![License](https://img.shields.io/badge/License-MIT-green)

---

## 📑 Mục Lục

- [Tính năng chính](#-tính-năng-chính)
- [Công nghệ sử dụng](#-công-nghệ-sử-dụng)
- [Kiến trúc dự án](#-kiến-trúc-dự-án)
- [Yêu cầu hệ thống](#-yêu-cầu-hệ-thống)
- [Cài đặt & Chạy local](#-cài-đặt--chạy-local)
- [Chạy kiểm thử](#-chạy-kiểm-thử)
- [Bảo mật](#-bảo-mật)
- [Giấy phép](#-giấy-phép)

---

## ✨ Tính Năng Chính

| Module | Trạng thái | Mô tả |
|--------|:----------:|-------|
| 🔐 Xác thực & Phân quyền | ✅ | Đăng nhập BCrypt, khóa tài khoản sau 5 lần sai, RBAC 6 quyền/menu |
| 👥 Quản lý Nhân viên | ✅ | CRUD, tìm kiếm, lọc phòng ban, import/export Excel, xóa/sửa hàng loạt |
| 🏢 Phòng ban & Chức vụ | ✅ | CRUD phòng ban, tổ nhóm, chức vụ với hệ số & phụ cấp |
| 📂 Danh mục & Ngày lễ | ✅ | Quản lý danh mục chung + 3-tier cache + CRUD ngày lễ VN |
| 📝 Hợp đồng lao động | ✅ | CRUD hợp đồng, mức lương HĐ, cảnh báo hết hạn, auto-deactivate NV |
| 📅 Biến động nhân sự | ✅ | Khen thưởng, kỷ luật, thăng chức, điều chuyển |
| ⏰ Chấm công | ✅ | Chấm công ngày, bảng công tháng, quản lý nghỉ phép (giới hạn 12 ngày/năm), auto chấm công khi duyệt phép |
| 💰 Tính lương | ✅ | BHXH/BHYT/BHTN, thuế TNCN 7 bậc, tạm ứng, phạt đi muộn, phiếu lương, hủy duyệt |
| 📊 Dashboard & Báo cáo | ✅ | KPI cards, biểu đồ GDI+, 8 loại báo cáo (kể cả BHXH phần DN), xuất CSV/Excel, in ấn |
| ⚙️ Hệ thống | ✅ | Quản lý user, menu, vai trò, audit log, sao lưu/phục hồi DB, thiết lập công ty |
| 🧪 Unit Tests | ✅ | xUnit + Moq, test thuế TNCN 7 bậc |

> 📄 Xem chi tiết **72 tính năng** tại [BaoCaoTinhNang.md](BaoCaoTinhNang.md)

---

## 🛠️ Công Nghệ Sử Dụng

### Nền tảng & Framework

| Thành phần | Công nghệ | Phiên bản | Mục đích |
|-----------|-----------|:---------:|---------|
| Runtime | .NET | 8.0 | Nền tảng chạy ứng dụng |
| UI Framework | Windows Forms | .NET 8 | Giao diện desktop Windows |
| Database | SQL Server | 2019+ | Lưu trữ dữ liệu (hỗ trợ Express / LocalDB) |
| Micro-ORM | Dapper | 2.1.35 | Truy vấn database hiệu năng cao |
| DB Driver | Microsoft.Data.SqlClient | 5.2.2 | Kết nối SQL Server |
| Testing | xUnit + Moq | 2.5+ | Unit testing framework |

### NuGet Packages

| Package | Layer | Mục đích |
|---------|:-----:|---------|
| `Dapper` | DAL | Micro-ORM, mapping SQL ↔ Object |
| `Microsoft.Data.SqlClient` | DAL | ADO.NET provider cho SQL Server |
| `BCrypt.Net-Next` | BLL | Hash mật khẩu an toàn (work factor 12) |
| `Newtonsoft.Json` | BLL, UI | Serialize/deserialize JSON (cache, config) |
| `ClosedXML` | UI | Import/Export file Excel (.xlsx) |
| `Microsoft.Extensions.DependencyInjection` | UI | Dependency Injection container |
| `Serilog` + `Serilog.Sinks.File` | UI | Structured logging |
| `Moq` | Tests | Mocking framework cho unit tests |

### Design Patterns & Architecture

| Pattern | Áp dụng |
|---------|---------|
| **3-Layer Architecture** | UI → BLL (Business Logic) → DAL (Data Access) |
| **Repository Pattern** | `BaseRepository<T>` + 17 repository classes với Dapper |
| **Service Layer** | 18 service classes, 4 service interfaces (`ISalaryService`, `ILeaveService`, `IAttendanceService`, `IContractService`) |
| **Dependency Injection** | `Microsoft.Extensions.DependencyInjection` đăng ký services + repositories |
| **JSON File Caching** | 3-tier cache: Memory → JSON File → Database (`JsonCacheManager`) |
| **RBAC Authorization** | Role → Menu → 6 Actions (View/Add/Edit/Delete/Export/Print) |
| **Result Pattern** | `Result<T>` cho response thống nhất (sẵn dùng cho features mới) |
| **Constants Pattern** | `StatusConstants` — centralized status strings, loại bỏ magic strings |
| **Guard Clauses** | `Guard.cs` — validation utility (NotNull, PositiveId, ValidMonth...) |
| **Batch Query** | `GetConfigDictionaryAsync()` — load tất cả configs trong 1 query |

---

## 🏗️ Kiến Trúc Dự Án

```
QuanLyNhanVien.sln
│
├── QuanLyNhanVien/                    # 🖥️ UI Layer (WinForms)
│   ├── Forms/
│   │   ├── Main/                      # FrmLogin, FrmMain
│   │   ├── Employee/                  # FrmEmployeeList, FrmEmployeeDetail,
│   │   │                              # FrmContractManager, FrmEmployeeEvents
│   │   ├── Department/                # FrmDepartmentList
│   │   ├── Position/                  # FrmPositionList
│   │   ├── Category/                  # FrmCategoryManager, FrmHolidayManager
│   │   ├── Permission/                # FrmMenuManager, FrmRoleList, FrmRolePermission
│   │   ├── Attendance/                # FrmAttendanceDaily, FrmAttendanceMonthly,
│   │   │                              # FrmLeaveRequest
│   │   ├── Salary/                    # FrmSalaryCalculation, FrmSalaryConfig,
│   │   │                              # FrmSalaryHistory, FrmPayslip, FrmAdvanceManager
│   │   ├── Report/                    # FrmDashboard, FrmReportExport
│   │   └── System/                    # FrmUserManager, FrmAuditLog, FrmBackupRestore,
│   │                                  # FrmChangePassword, FrmCompanySettings
│   ├── Helpers/
│   │   ├── AppSession.cs              # Session & user context hiện tại
│   │   ├── FormHelper.cs              # RBAC helper — kiểm tra quyền trên form
│   │   └── ThemeColors.cs             # Dark theme & color palette
│   ├── appsettings.example.json       # Template config
│   └── Program.cs                     # Entry point + DI registration
│
├── QuanLyNhanVien.BLL/                # ⚙️ Business Logic Layer
│   ├── Interfaces/                    # Service interfaces (ISalaryService, ...)
│   ├── Services/                      # 18 service classes
│   │   ├── SalaryService.cs           # Tính lương, BHXH, thuế TNCN, batch config
│   │   ├── AttendanceService.cs       # Chấm công, OT, DetermineStatus
│   │   ├── LeaveService.cs            # Nghỉ phép, giới hạn 12 ngày, auto chấm công
│   │   ├── ContractService.cs         # Hợp đồng, auto-deactivate NV hết hạn
│   │   ├── ReportService.cs           # Báo cáo NLĐ + NSDLĐ
│   │   └── ... (13 services khác)
│   └── Cache/
│       └── JsonCacheManager.cs        # 3-tier cache: Memory → JSON → DB
│
├── QuanLyNhanVien.DAL/                # 💾 Data Access Layer
│   ├── Context/
│   │   └── DbConnectionFactory.cs     # Tạo kết nối SQL Server
│   └── Repositories/                  # 17 repository classes
│       ├── BaseRepository.cs          # Base repository (generic CRUD)
│       ├── SalaryRepository.cs        # Lương + GetConfigDictionaryAsync (batch)
│       ├── HolidayRepository.cs       # Ngày lễ (mới)
│       └── ... (14 repositories khác)
│
├── QuanLyNhanVien.Models/             # 📦 Shared Models
│   ├── Entities/                      # 17 entity classes
│   │   ├── Employee.cs, Department.cs, Position.cs
│   │   ├── SalaryRecord.cs, SalaryConfig.cs, AttendanceRecord.cs
│   │   ├── Contract.cs, LeaveRequest.cs, Advance.cs
│   │   ├── User.cs, Role.cs, MenuNode.cs
│   │   ├── AuditLog.cs, Category.cs, CompanySetting.cs
│   │   ├── EmployeeEvent.cs
│   │   └── Holiday.cs                # Ngày lễ (mới)
│   ├── Common/                        # Architecture improvements
│   │   ├── Result.cs                  # Result<T> pattern
│   │   ├── StatusConstants.cs         # Status constants (loại bỏ magic strings)
│   │   └── Guard.cs                   # Guard clauses validation
│   └── DTOs/                          # CommonDtos, LoginDtos, PaginationResult
│
├── QuanLyNhanVien.Tests/              # 🧪 Unit Tests (xUnit + Moq)
│   └── SalaryServiceTests.cs          # 8 tests cho thuế TNCN 7 bậc
│
└── Database/                          # 🗄️ SQL Scripts (chạy theo thứ tự)
    ├── 01_CreateDatabase.sql          # Tạo database
    ├── 02_Tables.sql                  # Tạo bảng (17+ tables)
    ├── 04_SeedData.sql                # Dữ liệu gốc (admin, roles, menus)
    ├── 05–10 Phase migrations         # Menu + tables theo giai đoạn
    ├── 11_SampleData.sql              # Dữ liệu mẫu (50+ nhân viên)
    ├── 12_SalaryConfigFix.sql         # Fix EffectiveFrom/EffectiveTo
    ├── 13_BugFixes_AdvanceHoliday.sql # Cột AdvanceAmount + Bảng Holidays
    └── 14_MissFeatures.sql            # Cột ContractSalary
```

---

## 📋 Yêu Cầu Hệ Thống

| Yêu cầu | Chi tiết |
|---------|---------:|
| **Hệ điều hành** | Windows 10+ (x64) |
| **.NET SDK** | [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) trở lên |
| **SQL Server** | [SQL Server 2019+](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) — Express, LocalDB, hoặc Full |
| **IDE (tùy chọn)** | Visual Studio 2022+ / VS Code / Rider |

---

## 🚀 Cài Đặt & Chạy Local

### Bước 1: Clone repository

```bash
git clone https://github.com/Viet0904/QuanLyNhanVien.git
cd QuanLyNhanVien
```

### Bước 2: Cấu hình Connection String

```bash
copy QuanLyNhanVien\appsettings.example.json QuanLyNhanVien\appsettings.json
```

Mở `QuanLyNhanVien\appsettings.json` và sửa connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=QuanLyNhanVien;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "AppSettings": {
    "CacheExpiryMinutes": 60,
    "AutoLockMinutes": 30,
    "PageSize": 50
  }
}
```

**Ví dụ connection string:**

| Loại | Connection String |
|------|-------------------|
| SQL Express | `Server=.\SQLEXPRESS;Database=QuanLyNhanVien;Trusted_Connection=True;TrustServerCertificate=True;` |
| LocalDB | `Server=(localdb)\MSSQLLocalDB;Database=QuanLyNhanVien;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Server (IP) | `Server=192.168.1.100;Database=QuanLyNhanVien;User Id=sa;Password=YourPassword;TrustServerCertificate=True;` |
| Docker SQL | `Server=localhost,1433;Database=QuanLyNhanVien;User Id=sa;Password=YourPassword;TrustServerCertificate=True;` |

### Bước 3: Tạo Database & Seed dữ liệu

Chạy lần lượt các file SQL trong thư mục `Database/`:

```bash
# Tạo database + tables
sqlcmd -S YOUR_SERVER -i Database\01_CreateDatabase.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\02_Tables.sql

# Seed data (admin, roles, menus, permissions)
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\04_SeedData.sql

# Phase migrations (05 → 10)
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\05_Phase4_AttendanceMenus.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\06_Phase5_SalaryMenus.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\07_Phase6_ReportMenus.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\08_Phase7_SystemMenus.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\09_Phase8_CategorySeed.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\10_Phase9_ContractsEventsAdvances.sql

# Bug fixes & new features migrations
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\12_SalaryConfigFix.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\13_BugFixes_AdvanceHoliday.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\14_MissFeatures.sql

# (Tùy chọn) Dữ liệu mẫu 50+ nhân viên
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\11_SampleData.sql
```

> 💡 **Mẹo:** Nếu dùng SSMS, mở từng file SQL và nhấn **F5** (Execute) theo thứ tự.

### Bước 4: Restore NuGet packages

```bash
dotnet restore
```

### Bước 5: Chạy ứng dụng

```bash
dotnet run --project QuanLyNhanVien
```

### Bước 6: Đăng nhập

| Username | Password | Vai trò |
|----------|----------|---------|
| `admin` | `admin123` | Administrator (toàn quyền) |

> ⚠️ **Quan trọng:** Hãy đổi mật khẩu admin ngay sau lần đăng nhập đầu tiên.

---

## ✅ Chạy Kiểm Thử

### Build toàn bộ solution

```bash
dotnet build QuanLyNhanVien.sln
```

**Kết quả mong đợi:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Chạy Unit Tests

```bash
dotnet test QuanLyNhanVien.Tests --verbosity normal
```

**Kết quả mong đợi:**
```
Passed!  - Failed: 0, Passed: 8, Skipped: 0, Total: 8
```

**Test cases:**

| Test | Mô tả |
|------|-------|
| `CalculatePIT_ZeroIncome` | Thu nhập 0 → thuế 0 |
| `CalculatePIT_NegativeIncome` | Thu nhập âm → thuế 0 |
| `CalculatePIT_Bracket1_5Percent` | Bậc 1: 1M → 50K, 5M → 250K |
| `CalculatePIT_Bracket2_10Percent` | Bậc 2: 7M → 450K, 10M → 750K |
| `CalculatePIT_HigherBrackets` | Bậc 3-6: 18M/32M/52M/80M |
| `CalculatePIT_Bracket7_35Percent` | Bậc 7: 100M → 25.15M |
| `CalculatePIT_SmallAmount` | Rounding: 3.5M → 175K |

### Publish (đóng gói)

```bash
# Self-contained (không cần cài .NET trên máy đích)
dotnet publish QuanLyNhanVien -c Release -r win-x64 --self-contained true

# Framework-dependent (cần cài .NET 8 Runtime)
dotnet publish QuanLyNhanVien -c Release -r win-x64 --self-contained false
```

### Checklist kiểm thử thủ công

Sau khi build thành công và chạy ứng dụng, kiểm tra các chức năng:

- [ ] Đăng nhập với `admin` / `admin123`
- [ ] Xem danh sách nhân viên, tìm kiếm, lọc phòng ban
- [ ] Thêm / sửa / xóa nhân viên
- [ ] Import Excel → kiểm tra dữ liệu nhập
- [ ] Export danh sách nhân viên ra Excel
- [ ] Quản lý phòng ban, chức vụ, **ngày lễ**
- [ ] Chấm công ngày → xem bảng công tháng
- [ ] Tạo đơn nghỉ phép → phê duyệt → **kiểm tra auto chấm công**
- [ ] Tạo đơn phép > 12 ngày → **kiểm tra giới hạn**
- [ ] Tính lương tháng → xem phiếu lương → **kiểm tra trừ tạm ứng**
- [ ] Duyệt bảng lương → **kiểm tra hủy duyệt**
- [ ] Xem dashboard KPI, biểu đồ
- [ ] Xuất báo cáo CSV / Excel, **báo cáo BHXH phần DN**
- [ ] Quản lý user, phân quyền theo vai trò
- [ ] Sao lưu & phục hồi database

---

## 🔑 Bảo Mật

| Tính năng | Chi tiết |
|-----------|---------|
| **Mật khẩu** | Hash bằng BCrypt (work factor 12), không lưu plaintext |
| **Khóa tài khoản** | Tự động khóa sau 5 lần đăng nhập sai |
| **Phân quyền RBAC** | Role → Menu → 6 hành động: Xem / Thêm / Sửa / Xóa / Xuất / In |
| **Connection String** | Lưu trong `appsettings.json`, file này bị gitignore |
| **Audit Log** | Ghi log mọi thao tác quan trọng (ai, làm gì, lúc nào) |
| **Sensitive Files** | `.pfx`, `.key`, `.pem`, `secrets.json` bị gitignore |
| **Logging** | Serilog ghi structured log ra file, hỗ trợ debug & truy vết |
| **StatusConstants** | Loại bỏ magic strings, tránh lỗi typo trong status |
| **Guard Clauses** | Validate input đầu vào ở service layer |

---

## 📝 Giấy Phép

Dự án được cấp phép theo [MIT License](LICENSE).

```
MIT License — Copyright (c) 2026
```
