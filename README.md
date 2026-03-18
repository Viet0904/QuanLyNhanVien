# 📋 Quản Lý Nhân Viên — Employee Management System

> Ứng dụng quản lý nhân sự desktop xây dựng trên **.NET 8 WinForms**, **SQL Server**, **Dapper** micro-ORM, kiến trúc **3 lớp** (UI → BLL → DAL).

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![WinForms](https://img.shields.io/badge/UI-WinForms-0078D4?logo=windows)
![SQL Server](https://img.shields.io/badge/DB-SQL%20Server-CC2927?logo=microsoftsqlserver)
![Dapper](https://img.shields.io/badge/ORM-Dapper-FF6F00)
![License](https://img.shields.io/badge/License-MIT-green)

---

## 📑 Mục Lục

- [Tính năng chính](#-tính-năng-chính)
- [Công nghệ sử dụng](#-công-nghệ-sử-dụng)
- [Kiến trúc dự án](#-kiến-trúc-dự-án)
- [Yêu cầu hệ thống](#-yêu-cầu-hệ-thống)
- [Cài đặt & Chạy local](#-cài-đặt--chạy-local)
- [Chạy kiểm thử (Build Test)](#-chạy-kiểm-thử-build-test)
- [Bảo mật](#-bảo-mật)
- [Ảnh chụp màn hình](#-ảnh-chụp-màn-hình)
- [Giấy phép](#-giấy-phép)

---

## ✨ Tính Năng Chính

| Module | Trạng thái | Mô tả |
|--------|:----------:|-------|
| 🔐 Xác thực & Phân quyền | ✅ | Đăng nhập BCrypt, khóa tài khoản sau 5 lần sai, RBAC 6 quyền/menu |
| 👥 Quản lý Nhân viên | ✅ | CRUD, tìm kiếm, lọc phòng ban, import/export Excel, xóa hàng loạt |
| 🏢 Phòng ban & Chức vụ | ✅ | CRUD phòng ban, tổ nhóm, chức vụ với hệ số & phụ cấp |
| 📂 Danh mục | ✅ | Quản lý danh mục chung (trình độ, loại HĐ, ca làm việc...) + 3-tier cache |
| 📝 Hợp đồng lao động | ✅ | CRUD hợp đồng, cảnh báo hết hạn |
| 📅 Biến động nhân sự | ✅ | Khen thưởng, kỷ luật, thăng chức, điều chuyển |
| ⏰ Chấm công | ✅ | Chấm công ngày (giờ vào/ra), bảng công tháng, quản lý nghỉ phép |
| 💰 Tính lương | ✅ | Công thức lương, BHXH/BHYT/BHTN, thuế TNCN 7 bậc, tạm ứng, phiếu lương |
| 📊 Dashboard & Báo cáo | ✅ | KPI cards, biểu đồ GDI+, 7 loại báo cáo, xuất CSV/Excel, in ấn |
| ⚙️ Hệ thống | ✅ | Quản lý user, menu, vai trò, audit log, sao lưu/phục hồi DB, thiết lập công ty |

> 📄 Xem chi tiết 64 tính năng tại [BaoCaoTinhNang.md](BaoCaoTinhNang.md)

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

### NuGet Packages

| Package | Phiên bản | Layer | Mục đích |
|---------|:---------:|:-----:|---------|
| `Dapper` | 2.1.35 | DAL | Micro-ORM, mapping SQL ↔ Object |
| `Microsoft.Data.SqlClient` | 5.2.2 | DAL | ADO.NET provider cho SQL Server |
| `BCrypt.Net-Next` | 4.0.3 | BLL | Hash mật khẩu an toàn (work factor 12) |
| `Newtonsoft.Json` | 13.0.3 | BLL, UI | Serialize/deserialize JSON (cache, config) |
| `ClosedXML` | 0.105.0 | UI | Import/Export file Excel (.xlsx) |
| `Microsoft.Extensions.Configuration` | 8.0.0 | UI | Đọc file `appsettings.json` |
| `Microsoft.Extensions.Configuration.Json` | 8.0.1 | UI | JSON configuration provider |
| `Microsoft.Extensions.DependencyInjection` | 8.0.1 | UI | Dependency Injection container |
| `Serilog` | 4.0.0 | UI | Structured logging framework |
| `Serilog.Sinks.File` | 5.0.0 | UI | Ghi log ra file |

### Design Patterns

| Pattern | Áp dụng |
|---------|---------|
| **3-Layer Architecture** | UI → BLL (Business Logic) → DAL (Data Access) |
| **Repository Pattern** | `BaseRepository<T>` + 16 repository classes với Dapper |
| **Service Layer** | 17 service classes cách ly business logic |
| **Dependency Injection** | `Microsoft.Extensions.DependencyInjection` đăng ký services |
| **JSON File Caching** | 3-tier cache: Memory → JSON File → Database (qua `JsonCacheManager`) |
| **RBAC Authorization** | Role → Menu → 6 Actions (View/Add/Edit/Delete/Export/Print) |

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
│   │   ├── Category/                  # FrmCategoryManager
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
│   ├── Data/                          # JSON cache files (gitignored)
│   ├── appsettings.json               # Config (gitignored)
│   ├── appsettings.example.json       # Template config
│   └── Program.cs                     # Entry point + DI registration
│
├── QuanLyNhanVien.BLL/                # ⚙️ Business Logic Layer
│   ├── Services/                      # 17 service classes
│   │   ├── AuthService.cs             # Đăng nhập, BCrypt, khóa tài khoản
│   │   ├── EmployeeService.cs         # CRUD nhân viên
│   │   ├── SalaryService.cs           # Tính lương, BHXH, thuế TNCN
│   │   ├── AttendanceService.cs       # Chấm công, OT
│   │   ├── LeaveService.cs            # Quản lý nghỉ phép
│   │   ├── ReportService.cs           # Báo cáo tổng hợp
│   │   └── ... (11 services khác)
│   └── Cache/
│       └── JsonCacheManager.cs        # 3-tier cache: Memory → JSON → DB
│
├── QuanLyNhanVien.DAL/                # 💾 Data Access Layer
│   ├── Context/
│   │   └── DbConnectionFactory.cs     # Tạo kết nối SQL Server
│   └── Repositories/                  # 16 repository classes
│       ├── BaseRepository.cs          # Base repository với Dapper (generic CRUD)
│       ├── EmployeeRepository.cs      # Truy vấn nhân viên
│       ├── SalaryRepository.cs        # Truy vấn lương
│       ├── ReportRepository.cs        # Truy vấn báo cáo
│       └── ... (12 repositories khác)
│
├── QuanLyNhanVien.Models/             # 📦 Shared Models
│   ├── Entities/                      # 15 entity classes
│   │   ├── Employee.cs, Department.cs, Position.cs
│   │   ├── SalaryRecord.cs, AttendanceRecord.cs
│   │   ├── Contract.cs, LeaveRequest.cs, Advance.cs
│   │   ├── User.cs, Role.cs, MenuNode.cs
│   │   ├── AuditLog.cs, Category.cs, CompanySetting.cs
│   │   └── EmployeeEvent.cs
│   ├── DTOs/                          # CommonDtos, LoginDtos
│   ├── Enums/                         # PermissionType, AttendanceStatus,
│   │                                  # LeaveType, SalaryStatus
│   └── Constants/                     # AppConstants
│
└── Database/                          # 🗄️ SQL Scripts (chạy theo thứ tự)
    ├── 01_CreateDatabase.sql          # Tạo database
    ├── 02_Tables.sql                  # Tạo bảng (15+ tables)
    ├── 04_SeedData.sql                # Dữ liệu gốc (admin, roles, menus)
    ├── 05_Phase4_AttendanceMenus.sql  # Menu chấm công
    ├── 06_Phase5_SalaryMenus.sql      # Menu lương + tables
    ├── 07_Phase6_ReportMenus.sql      # Menu báo cáo
    ├── 08_Phase7_SystemMenus.sql      # Menu hệ thống
    ├── 09_Phase8_CategorySeed.sql     # Seed danh mục
    ├── 10_Phase9_ContractsEventsAdvances.sql  # Hợp đồng, biến động, tạm ứng
    └── 11_SampleData.sql              # Dữ liệu mẫu (50+ nhân viên)
```

---

## 📋 Yêu Cầu Hệ Thống

| Yêu cầu | Chi tiết |
|---------|---------|
| **Hệ điều hành** | Windows 10+ (x64) |
| **.NET SDK** | [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) trở lên |
| **SQL Server** | [SQL Server 2019+](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) — hỗ trợ Express, LocalDB, hoặc Full |
| **IDE (tùy chọn)** | Visual Studio 2022+ / VS Code / Rider |

---

## 🚀 Cài Đặt & Chạy Local

### Bước 1: Clone repository

```bash
git clone https://github.com/Viet0904/QuanLyNhanVien.git
cd QuanLyNhanVien
```

### Bước 2: Cấu hình Connection String

Sao chép file cấu hình mẫu:

```bash
copy QuanLyNhanVien\appsettings.example.json QuanLyNhanVien\appsettings.json
```

Mở `QuanLyNhanVien\appsettings.json` và sửa connection string theo SQL Server của bạn:

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

**Ví dụ connection string theo từng loại SQL Server:**

| Loại | Connection String |
|------|-------------------|
| SQL Express | `Server=.\SQLEXPRESS;Database=QuanLyNhanVien;Trusted_Connection=True;TrustServerCertificate=True;` |
| LocalDB | `Server=(localdb)\MSSQLLocalDB;Database=QuanLyNhanVien;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Server (IP) | `Server=192.168.1.100;Database=QuanLyNhanVien;User Id=sa;Password=YourPassword;TrustServerCertificate=True;` |
| Docker SQL | `Server=localhost,1433;Database=QuanLyNhanVien;User Id=sa;Password=YourPassword;TrustServerCertificate=True;` |

### Bước 3: Tạo Database & Seed dữ liệu

Chạy lần lượt các file SQL trong thư mục `Database/`. Có thể dùng **SSMS**, **Azure Data Studio**, hoặc **sqlcmd**:

```bash
# Tạo database
sqlcmd -S YOUR_SERVER -i Database\01_CreateDatabase.sql

# Tạo tables (15+ bảng)
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\02_Tables.sql

# Seed dữ liệu gốc (admin, roles, menus, permissions)
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\04_SeedData.sql

# (Tuỳ chọn) Chạy các migration bổ sung
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\05_Phase4_AttendanceMenus.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\06_Phase5_SalaryMenus.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\07_Phase6_ReportMenus.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\08_Phase7_SystemMenus.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\09_Phase8_CategorySeed.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\10_Phase9_ContractsEventsAdvances.sql

# (Tuỳ chọn) Thêm dữ liệu mẫu (50+ nhân viên để test)
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\11_SampleData.sql
```

> **💡 Mẹo:** Nếu dùng SSMS, mở từng file SQL và nhấn **F5** (Execute) theo thứ tự từ `01` → `11`.

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

> ⚠️ **Quan trọng:** Hãy đổi mật khẩu admin ngay sau lần đăng nhập đầu tiên qua menu **Hệ thống → Đổi mật khẩu**.

---

## ✅ Chạy Kiểm Thử (Build Test)

### Build toàn bộ solution

```bash
# Build ở chế độ Debug
dotnet build QuanLyNhanVien.sln

# Build ở chế độ Release
dotnet build QuanLyNhanVien.sln -c Release
```

**Kết quả mong đợi:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Kiểm tra từng project riêng lẻ

```bash
# Build Models (không phụ thuộc gì)
dotnet build QuanLyNhanVien.Models

# Build DAL (phụ thuộc Models)
dotnet build QuanLyNhanVien.DAL

# Build BLL (phụ thuộc DAL + Models)
dotnet build QuanLyNhanVien.BLL

# Build UI (phụ thuộc tất cả)
dotnet build QuanLyNhanVien
```

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
- [ ] Quản lý phòng ban, chức vụ
- [ ] Chấm công ngày → xem bảng công tháng
- [ ] Tạo đơn nghỉ phép → phê duyệt
- [ ] Tính lương tháng → xem phiếu lương
- [ ] Xem dashboard KPI, biểu đồ
- [ ] Xuất báo cáo CSV / Excel
- [ ] Quản lý user, phân quyền theo vai trò
- [ ] Sao lưu & phục hồi database
- [ ] Đổi mật khẩu

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

---

## 📸 Ảnh Chụp Màn Hình

> *Sẽ cập nhật sau*

---

## 📝 Giấy Phép

Dự án được cấp phép theo [MIT License](LICENSE).

```
MIT License — Copyright (c) 2026
```
