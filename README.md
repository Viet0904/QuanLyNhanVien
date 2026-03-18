# 📋 Employee Management System (Quản Lý Nhân Viên)

> Desktop HR management application built with **.NET 8 WinForms**, **SQL Server**, **Dapper** micro-ORM, and **3-Layer Architecture** (UI → BLL → DAL).

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![WinForms](https://img.shields.io/badge/UI-WinForms-0078D4?logo=windows)
![SQL Server](https://img.shields.io/badge/DB-SQL%20Server-CC2927?logo=microsoftsqlserver)
![Dapper](https://img.shields.io/badge/ORM-Dapper-FF6F00)
![License](https://img.shields.io/badge/License-MIT-green)

---

## ✨ Features

| Module | Status | Description |
|--------|:------:|-------------|
| 🔐 Authentication | ✅ | BCrypt password hashing, account lockout after 5 failed attempts |
| 👥 Employee CRUD | ✅ | Search, department filter, server-side pagination |
| 🏢 Department CRUD | ✅ | Soft delete, validation before deletion |
| 🎖️ Position CRUD | ✅ | Level hierarchy + Allowance management |
| 🛡️ RBAC Permissions | ✅ | 6 permissions per menu: View / Add / Edit / Delete / Export / Print |
| ☰ Menu Management | ✅ | Dynamic menu CRUD with drag-and-drop ordering |
| 📂 Category + Cache | ✅ | 3-tier caching: Memory → JSON File → Database |
| ⏰ Attendance | ✅ | Daily check-in/out, monthly report, leave requests |
| 💰 Payroll | ✅ | Salary config, monthly calculation, payslip, PIT |
| 📊 Reports & Dashboard | ✅ | KPI dashboard, GDI+ charts, CSV/Print export |

---

## 🏗️ Architecture

```
QuanLyNhanVien.sln
├── QuanLyNhanVien/              # 🖥️ UI Layer (WinForms)
│   ├── Forms/
│   │   ├── Main/                # Login + Main shell
│   │   ├── Employee/            # Employee list + detail
│   │   ├── Department/          # Department management
│   │   ├── Position/            # Position management
│   │   ├── Permission/          # RBAC + Menu management
│   │   └── Category/            # Category management
│   ├── Helpers/                 # FormHelper (RBAC), AppSession
│   └── appsettings.json         # Config (gitignored)
│
├── QuanLyNhanVien.BLL/          # ⚙️ Business Logic Layer
│   ├── Services/                # AuthService, EmployeeService, ...
│   └── Cache/                   # JsonCacheManager (3-tier)
│
├── QuanLyNhanVien.DAL/          # 💾 Data Access Layer
│   ├── Context/                 # DbConnectionFactory
│   └── Repositories/            # BaseRepo + 6 repositories (Dapper)
│
├── QuanLyNhanVien.Models/       # 📦 Shared Models
│   ├── Entities/                # 10 entity classes
│   ├── DTOs/                    # PaginationResult, MenuPermissionDto
│   ├── Enums/                   # PermissionType, AttendanceStatus
│   └── Constants/               # AppConstants
│
└── Database/                    # 🗄️ SQL Scripts
    ├── 01_CreateDatabase.sql
    ├── 02_Tables.sql
    └── 04_SeedData.sql
```

### Key Design Patterns
- **Repository Pattern** — All data access through `BaseRepository<T>` with Dapper
- **Service Layer** — Business logic isolated from UI and data access
- **JSON File Caching** — 3-tier cache (Memory → JSON File → DB) reduces database round-trips
- **RBAC Authorization** — Role-based access control with 6 granular permission types per menu item

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server 2019+](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Express / LocalDB)

### 1. Clone

```bash
git clone https://github.com/YOUR_USERNAME/employee-management-system.git
cd employee-management-system
```

### 2. Configure Database Connection

```bash
copy QuanLyNhanVien\appsettings.example.json QuanLyNhanVien\appsettings.json
```

Edit `QuanLyNhanVien\appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=QuanLyNhanVien;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Create Database

Run the SQL scripts in order:
```bash
# Using SSMS or sqlcmd:
sqlcmd -S YOUR_SERVER -i Database\01_CreateDatabase.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\02_Tables.sql
sqlcmd -S YOUR_SERVER -d QuanLyNhanVien -i Database\04_SeedData.sql
```

### 4. Run

```bash
dotnet run --project QuanLyNhanVien
```

### 5. Login

| Username | Password | Role |
|----------|----------|------|
| `admin` | `admin123` | Administrator (full access) |

> ⚠️ **Change the default admin password** after first login.

---

## 🔑 Security

| Feature | Implementation |
|---------|---------------|
| Password Storage | BCrypt hash (work factor 12), no plaintext |
| Account Lockout | Auto-lock after 5 failed login attempts |
| Authorization | Dynamic RBAC: Role → Menu → 6 Actions |
| Connection String | Stored in `appsettings.json` (gitignored) |
| Sensitive Files | `.pfx`, `.key`, `.pem` excluded via `.gitignore` |

---

## 📦 Tech Stack

| Component | Technology | Purpose |
|-----------|-----------|---------|
| UI Framework | WinForms (.NET 8) | Desktop GUI |
| Database | SQL Server 2019+ | Data storage |
| ORM | Dapper | Lightweight data access |
| Auth | BCrypt.Net-Next | Password hashing |
| Config | Microsoft.Extensions.Configuration | App settings |
| Serialization | Newtonsoft.Json | JSON caching |

---

## 📝 License

This project is licensed under the [MIT License](LICENSE).
