# 📊 BÁO CÁO TÍNH NĂNG CHI TIẾT — Quản Lý Nhân Viên

**Ngày cập nhật:** 18/03/2026  
**Tổng tính năng:** 64 | **✅ Hoàn thành:** 55 (~86%) | **⚠️ Một phần:** 4 | **❌ Chưa làm:** 5

---

## 📑 Mục Lục

1. [Menu Hệ Thống (System)](#1-menu-hệ-thống-system--1111-100)
2. [Menu Danh Mục (Categories)](#2-menu-danh-mục-categories--66-100)
3. [Menu Nhân Sự (HR)](#3-menu-nhân-sự-hr--1012-83)
4. [Menu Chấm Công (Timekeeping)](#4-menu-chấm-công-timekeeping--57-71)
5. [Menu Tính Lương (Payroll)](#5-menu-tính-lương-payroll--1214-86)
6. [Menu Báo Cáo & Dashboard](#6-menu-báo-cáo--dashboard--1214-86)
7. [Tổng Kết](#-tổng-kết)
8. [Tính năng chưa hoàn thành](#-tính-năng-chưa-làm-5)
9. [Tính năng một phần](#️-tính-năng-một-phần-4)

---

## 1. Menu Hệ Thống (System) — ✅ 11/11 (100%)

### 1.1 Xác thực & Đăng nhập

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 1 | Đăng nhập | ✅ | `FrmLogin.cs`, `AuthService.cs` | Hash mật khẩu BCrypt (work factor 12), khóa tài khoản sau 5 lần sai, hiển thị thông báo lỗi chi tiết |
| 2 | Đăng xuất | ✅ | `FrmMain.cs` | Nút đăng xuất trong main shell, xóa session, quay về màn hình login |
| 3 | Đổi mật khẩu | ✅ | `FrmChangePassword.cs`, `AuthService.cs` | Yêu cầu nhập mật khẩu cũ + mật khẩu mới, validate độ mạnh, hash BCrypt |

### 1.2 Quản lý Người dùng & Phân quyền

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 4 | Quản lý người dùng (CRUD) | ✅ | `FrmUserManager.cs`, `UserService.cs`, `UserRepository.cs` | Thêm / sửa / xóa tài khoản, liên kết với nhân viên, gán vai trò, kiểm tra trùng username |
| 5 | Phân quyền — Quản lý vai trò | ✅ | `FrmRoleList.cs`, `MenuRoleRepository.cs` | CRUD vai trò (Role), mỗi vai trò gán bộ quyền riêng |
| 6 | Phân quyền — Gán quyền theo menu | ✅ | `FrmRolePermission.cs` | TreeView menu + 6 checkbox quyền/menu: Xem, Thêm, Sửa, Xóa, Xuất, In |
| 7 | Phân quyền — Gán role cho user | ✅ | `FrmUserManager.cs` | CheckedListBox để gán nhiều role cho 1 user |

### 1.3 Quản lý Hệ thống

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 8 | Quản lý Menu | ✅ | `FrmMenuManager.cs`, `MenuService.cs` | CRUD menu items, sắp xếp thứ tự, hỗ trợ menu cha-con (tree structure) |
| 9 | Nhật ký hệ thống (Audit Log) | ✅ | `FrmAuditLog.cs`, `AuditService.cs`, `AuditLogRepository.cs` | Ghi nhận mọi thao tác (người thực hiện, hành động, thời điểm, chi tiết), lọc theo ngày/user |
| 10 | Sao lưu & Phục hồi DB | ✅ | `FrmBackupRestore.cs`, `BackupService.cs` | Backup database ra file `.bak`, restore từ file, chọn thư mục lưu |
| 11 | Thiết lập chung (công ty, logo) | ✅ | `FrmCompanySettings.cs`, `CompanySettingsService.cs` | Cấu hình tên công ty, địa chỉ, logo — hiển thị trên phiếu lương & báo cáo |

---

## 2. Menu Danh Mục (Categories) — ✅ 6/6 (100%)

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 1 | Phòng ban / Tổ nhóm | ✅ | `FrmDepartmentList.cs`, `DepartmentService.cs`, `DepartmentRepository.cs` | CRUD phòng ban, soft delete, kiểm tra nhân viên thuộc phòng ban trước khi xóa |
| 2 | Chức vụ / Chức danh | ✅ | `FrmPositionList.cs`, `PositionService.cs`, `PositionRepository.cs` | CRUD chức vụ, cấp bậc (Level), phụ cấp chức vụ (`AllowanceAmount`) |
| 3 | Trình độ học vấn | ✅ | Seed data `EDUCATION_LEVEL` | 6 lựa chọn: Sau đại học, Đại học, Cao đẳng, Trung cấp, THPT, Khác |
| 4 | Loại hợp đồng | ✅ | Seed data `CONTRACT_TYPE` | 4 loại: Thử việc, Có thời hạn, Không thời hạn, Thời vụ |
| 5 | Ca làm việc | ✅ | Seed data `SHIFT_TYPE` | Ca sáng, ca chiều, ca đêm, hành chính |
| 6 | Quản lý danh mục chung | ✅ | `FrmCategoryManager.cs`, `CategoryService.cs`, `CategoryRepository.cs` | Giao diện quản lý chung cho tất cả danh mục, hỗ trợ cache 3 tầng (Memory → JSON → DB) |

**🔹 Cơ chế Cache 3 tầng:**
```
Đọc danh mục → Memory Cache (nhanh nhất)
    ↓ miss
→ JSON File Cache (Data/categories.json)
    ↓ miss
→ Database (SQL Server)
    ↑ cập nhật ngược lại cache
```
Thời gian hết hạn cache có thể cấu hình qua `AppSettings.CacheExpiryMinutes` trong `appsettings.json`.

---

## 3. Menu Nhân Sự (HR) — ✅ 10/12 (83%)

### 3.1 Quản lý Nhân viên

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 1 | Danh sách NV (lọc, tìm kiếm) | ✅ | `FrmEmployeeList.cs`, `EmployeeService.cs`, `EmployeeRepository.cs` | DataGridView, tìm kiếm theo tên/mã NV, lọc theo phòng ban, server-side pagination |
| 2 | Thêm / Sửa hồ sơ | ✅ | `FrmEmployeeDetail.cs` | Form 4 tabs: Thông tin chung, Liên hệ, Tài chính, Ghi chú. Validate đầy đủ (CCCD, email, SĐT) |
| 3 | Xóa hồ sơ (soft delete) | ✅ | `EmployeeService.cs` | Đánh dấu `IsDeleted = true`, không xóa vật lý, giữ lại để tra cứu |
| 4 | Xóa hàng loạt (Batch Delete) | ✅ | `FrmEmployeeList.cs` | Checkbox multi-select rows + nút xóa hàng loạt, xác nhận trước khi xóa |
| 5 | Import Excel | ✅ | `FrmEmployeeList.cs` + ClosedXML | Nhập danh sách từ file `.xlsx`, validate dữ liệu dòng, báo lỗi chi tiết |
| 6 | Export Excel | ✅ | `FrmEmployeeList.cs` + ClosedXML | Xuất danh sách nhân viên ra `.xlsx`, tùy chọn cột |

### 3.2 Thông tin Chi tiết

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 7 | Thông tin cá nhân + ngân hàng | ✅ | `FrmEmployeeDetail.cs` (Tab 1 + Tab 3) | Họ tên, ngày sinh, CCCD, địa chỉ, SĐT, email, số tài khoản, ngân hàng |
| 8 | Người phụ thuộc | ✅ | `FrmEmployeeDetail.cs` (Tab Tài chính) | Trường `NumberOfDependents` (NumericUpDown) — dùng để giảm trừ thuế TNCN (4.4 triệu/NPT) |
| 9 | Hợp đồng lao động | ✅ | `FrmContractManager.cs`, `ContractService.cs`, `ContractRepository.cs` | CRUD hợp đồng, liên kết nhân viên, cảnh báo hợp đồng sắp hết hạn (30 ngày) |
| 10 | Biến động nhân sự | ✅ | `FrmEmployeeEvents.cs`, `EmployeeEventService.cs`, `EmployeeEventRepository.cs` | Ghi nhận sự kiện: Khen thưởng, Kỷ luật, Thăng chức, Điều chuyển — lịch sử đầy đủ |

### 3.3 Chưa hoàn thành

| # | Tính năng | Trạng thái | Ghi chú |
|:-:|-----------|:---:|---------|
| 11 | Sửa hàng loạt (Batch Edit) | ❌ | Cần thiết kế UI multi-select + chọn trường cần sửa |
| 12 | Gửi phiếu lương qua email | ❌ | Cần tích hợp SMTP server |

---

## 4. Menu Chấm Công (Timekeeping) — ✅ 5/7 (71%)

### 4.1 Hoàn thành

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 1 | Chấm công ngày (giờ vào/ra) | ✅ | `FrmAttendanceDaily.cs`, `AttendanceService.cs`, `AttendanceRepository.cs` | Nhập giờ CheckIn/CheckOut, tự động tính giờ làm việc, OT |
| 2 | Quản lý nghỉ phép | ✅ | `FrmLeaveRequest.cs`, `LeaveService.cs`, `LeaveRequestRepository.cs` | Tạo đơn nghỉ phép (Phép năm, Ốm, Thai sản, Không lương), phê duyệt/từ chối, kiểm tra số ngày phép còn |
| 3 | Bảng công tổng hợp tháng | ✅ | `FrmAttendanceMonthly.cs` | Tổng hợp ngày công, ngày nghỉ, giờ OT theo tháng — dữ liệu đầu vào cho tính lương |
| 4 | Tính giờ OT | ✅ | `AttendanceService.cs` → `SalaryService.cs` | `OvertimeHours` được tính từ chấm công, nhân hệ số 1.5x/2x khi tính lương |
| 5 | Chấm công hàng loạt | ⚠️ | `FrmAttendanceDaily.cs` | Chấm theo phòng ban/ngày, nhưng chưa hỗ trợ batch cho nhiều NV cùng 1 lúc |

### 4.2 Chưa hoàn thành

| # | Tính năng | Trạng thái | Ghi chú |
|:-:|-----------|:---:|---------|
| 6 | Tự động phát hiện đi muộn/về sớm | ⚠️ | Có status `Late` nhưng chưa tự động so sánh với giờ ca làm việc |
| 7 | Kết nối máy chấm công | ❌ | Cần SDK thiết bị cụ thể (ZKTeco, Suprema, v.v.) |

---

## 5. Menu Tính Lương (Payroll) — ✅ 12/14 (86%)

### 5.1 Cấu hình & Công thức

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 1 | Thiết lập công thức & định mức | ✅ | `FrmSalaryConfig.cs`, `SalaryService.cs` | Cấu hình lương cơ sở, ngày công chuẩn, tỉ lệ BHXH/BHYT/BHTN |
| 2 | Hệ số lương | ✅ | `Employee.SalaryCoefficient` | Mỗi nhân viên có hệ số riêng, nhân với lương cơ sở |
| 3 | Phụ cấp chức vụ | ✅ | `Position.AllowanceAmount` | Phụ cấp tự động theo chức vụ hiện tại |

### 5.2 Tính toán

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 4 | BHXH / BHYT / BHTN | ✅ | `SalaryService.CalculateMonthlyAsync()` | Tính tự động: BHXH 8%, BHYT 1.5%, BHTN 1% (phần NLĐ) |
| 5 | Thuế TNCN lũy tiến 7 bậc | ✅ | `SalaryService.CalculatePIT()` | Đúng biểu thuế Việt Nam: 5%, 10%, 15%, 20%, 25%, 30%, 35% |
| 6 | Giảm trừ bản thân + NPT | ✅ | `SalaryService.cs` | Giảm trừ 11 triệu/bản thân + 4.4 triệu/người phụ thuộc |
| 7 | Lập bảng lương tháng tự động | ✅ | `SalaryService.CalculateMonthlyAsync()`, `SalaryRepository.cs` | Tính lương hàng loạt cho toàn bộ nhân viên theo tháng, dựa trên ngày công thực tế |

### 5.3 Quản lý & Xuất

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 8 | Quản lý tạm ứng | ✅ | `FrmAdvanceManager.cs`, `AdvanceService.cs`, `AdvanceRepository.cs` | Tạo phiếu tạm ứng, duyệt, trừ vào lương tháng |
| 9 | In / Xuất phiếu lương | ✅ | `FrmPayslip.cs` | Phiếu lương chi tiết từng NV, hỗ trợ in & export |
| 10 | Khóa bảng lương (Approved) | ✅ | `SalaryService.ApproveAllAsync()` | Khóa bảng lương sau khi duyệt, ngăn chỉnh sửa |
| 11 | Lịch sử lương | ✅ | `FrmSalaryHistory.cs` | Tra cứu lương theo NV & khoảng thời gian |

### 5.4 Chưa hoàn thành

| # | Tính năng | Trạng thái | Ghi chú |
|:-:|-----------|:---:|---------|
| 12 | Phụ cấp khác (ăn trưa, đi lại) | ⚠️ | `OtherAllowance = 0` hardcode, cần UI config cho phụ cấp ăn trưa, xăng xe, đi lại |
| 13 | Khấu trừ (phạt đi muộn) | ⚠️ | `OtherDeductions = 0` hardcode, cần UI config quy tắc khấu trừ |
| 14 | Gửi phiếu lương qua email | ❌ | Cần SMTP server config + template email HTML |

**🔹 Công thức tính lương:**
```
Lương thực nhận = Lương cơ bản × Hệ số × (Ngày công / Ngày chuẩn)
                + Phụ cấp chức vụ
                + Phụ cấp khác
                + Lương OT (giờ OT × hệ số OT × lương giờ)
                - BHXH (8%) - BHYT (1.5%) - BHTN (1%)
                - Thuế TNCN (7 bậc lũy tiến)
                - Tạm ứng
                - Khấu trừ khác
```

---

## 6. Menu Báo Cáo & Dashboard — ✅ 12/14 (86%)

### 6.1 Dashboard

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 1 | Dashboard KPI cards | ✅ | `FrmDashboard.cs` | 4 KPI: Tổng NV, NV mới tháng, NV nghỉ việc, Quỹ lương tháng |
| 2 | Biểu đồ NV theo phòng ban | ✅ | `FrmDashboard.cs` (GDI+ custom) | Bar chart hiển thị số lượng NV mỗi phòng ban |
| 3 | Biểu đồ chi lương theo tháng | ✅ | `FrmDashboard.cs` (GDI+ custom) | Bar chart 12 tháng, so sánh chi phí lương |
| 4 | Top 5 lương cao nhất | ✅ | `FrmDashboard.cs` | Bảng ranking nhân viên lương cao nhất |

### 6.2 Báo cáo

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 5 | Báo cáo danh sách NV | ✅ | `FrmReportExport.cs`, `ReportService.cs`, `ReportRepository.cs` | Danh sách toàn bộ nhân viên, lọc theo phòng ban/trạng thái |
| 6 | Báo cáo NV mới / nghỉ việc | ✅ | `FrmReportExport.cs` | Báo cáo nhân viên vào/ra theo khoảng thời gian |
| 7 | Báo cáo sinh nhật trong tháng | ✅ | `FrmReportExport.cs` | Danh sách NV có sinh nhật trong tháng được chọn |
| 8 | Báo cáo bảng lương chi tiết | ✅ | `FrmReportExport.cs` | Chi tiết lương từng NV theo tháng: lương gộp, BHXH, thuế, thực nhận |
| 9 | Báo cáo BHXH | ✅ | `FrmReportExport.cs` | Tổng hợp BHXH/BHYT/BHTN phần NLĐ & NSDLĐ |
| 10 | Báo cáo thuế TNCN | ✅ | `FrmReportExport.cs` | Chi tiết thuế TNCN từng NV: thu nhập chịu thuế, giảm trừ, thuế phải nộp |
| 11 | Biến động nhân sự | ✅ | `FrmReportExport.cs` | Tỉ lệ vào/ra, thống kê biến động theo khoảng thời gian |

### 6.3 Xuất & In

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 12 | Xuất CSV + Excel | ✅ | `FrmReportExport.cs` + ClosedXML | Xuất báo cáo ra `.csv` hoặc `.xlsx`, tự động format |
| 13 | In báo cáo | ✅ | `FrmReportExport.cs` | PrintPreview + Print, hỗ trợ thiết lập trang |

### 6.4 Chưa hoàn thành

| # | Tính năng | Trạng thái | Ghi chú |
|:-:|-----------|:---:|---------|
| 14 | Báo cáo chấm công (đi muộn thường xuyên) | ❌ | Có chấm công tháng nhưng chưa phân tích tần suất đi muộn chi tiết |

---

## 📈 Tổng Kết

| Menu | ✅ Hoàn thành | ⚠️ Một phần | ❌ Chưa làm | Tổng | Tỉ lệ |
|------|:-----------:|:----------:|:----------:|:----:|:-----:|
| 🔧 Hệ thống | 11 | 0 | 0 | 11 | **100%** |
| 📂 Danh mục | 6 | 0 | 0 | 6 | **100%** |
| 👥 Nhân sự | 10 | 0 | 2 | 12 | **83%** |
| ⏰ Chấm công | 4 | 2 | 1 | 7 | **71%** |
| 💰 Tính lương | 11 | 2 | 1 | 14 | **86%** |
| 📊 Báo cáo | 13 | 0 | 1 | 14 | **93%** |
| **TỔNG CỘNG** | **55** | **4** | **5** | **64** | **86%** |

### Biểu đồ tiến độ

```
Hệ thống   ████████████████████ 100%  (11/11)
Danh mục   ████████████████████ 100%  (6/6)
Báo cáo    ██████████████████▓░  93%  (13/14)
Tính lương █████████████████░░░  86%  (12/14)
Nhân sự    ████████████████░░░░  83%  (10/12)
Chấm công  ██████████████░░░░░░  71%  (5/7)
```

---

## ❌ Tính Năng Chưa Làm (5)

| # | Tính năng | Module | Ưu tiên | Lý do / Phụ thuộc |
|:-:|-----------|--------|:-------:|-------------------|
| 1 | Sửa hàng loạt (Batch Edit) | Nhân sự | Trung bình | Cần UI phức tạp: multi-select rows + form chọn trường cần sửa + preview changes |
| 2 | Gửi phiếu lương qua email | Tính lương | Cao | Cần cấu hình SMTP server, template email HTML, queue gửi hàng loạt |
| 3 | Kết nối máy chấm công | Chấm công | Thấp | Phụ thuộc SDK thiết bị cụ thể (ZKTeco, Suprema, HikVision...), cần thiết bị vật lý để test |
| 4 | Gửi email (chung) | Hệ thống | Cao | Cần SMTP config UI trong `FrmCompanySettings`, service gửi mail + retry logic |
| 5 | Phân tích đi muộn thường xuyên | Báo cáo | Trung bình | Cần logic so sánh `CheckIn` với giờ bắt đầu ca, thống kê tần suất, ngưỡng cảnh báo |

---

## ⚠️ Tính Năng Một Phần (4)

| # | Tính năng | Module | Hiện trạng | Cần bổ sung |
|:-:|-----------|--------|-----------|-------------|
| 1 | Chấm công hàng loạt | Chấm công | Chấm theo phòng ban/ngày | UI batch cho nhiều NV cùng lúc (DataGridView inline edit) |
| 2 | Tự động phát hiện đi muộn/về sớm | Chấm công | Có status `Late` thủ công | So sánh `CheckIn`/`CheckOut` với giờ ca tự động + cảnh báo |
| 3 | Phụ cấp khác | Tính lương | `OtherAllowance = 0` hardcode | UI config: ăn trưa, đi lại, xăng xe, nhà ở — mỗi khoản riêng |
| 4 | Khấu trừ phạt đi muộn | Tính lương | `OtherDeductions = 0` hardcode | UI config quy tắc: phạt X đồng nếu đi muộn Y lần/tháng |

---

## 📊 Thống Kê Code

### Forms (27 forms)

| Thư mục | Số form | Forms |
|---------|:-------:|-------|
| `Main` | 2 | FrmLogin, FrmMain |
| `Employee` | 4 | FrmEmployeeList, FrmEmployeeDetail, FrmContractManager, FrmEmployeeEvents |
| `Department` | 1 | FrmDepartmentList |
| `Position` | 1 | FrmPositionList |
| `Category` | 1 | FrmCategoryManager |
| `Permission` | 3 | FrmMenuManager, FrmRoleList, FrmRolePermission |
| `Attendance` | 3 | FrmAttendanceDaily, FrmAttendanceMonthly, FrmLeaveRequest |
| `Salary` | 5 | FrmSalaryCalculation, FrmSalaryConfig, FrmSalaryHistory, FrmPayslip, FrmAdvanceManager |
| `Report` | 2 | FrmDashboard, FrmReportExport |
| `System` | 5 | FrmUserManager, FrmAuditLog, FrmBackupRestore, FrmChangePassword, FrmCompanySettings |

### Services (17 services)

| Service | Chức năng chính |
|---------|----------------|
| `AuthService` | Xác thực, BCrypt hash, khóa tài khoản |
| `EmployeeService` | CRUD nhân viên, pagination, search |
| `DepartmentService` | CRUD phòng ban, validate trước xóa |
| `PositionService` | CRUD chức vụ, phụ cấp |
| `CategoryService` | Danh mục + cache 3 tầng |
| `MenuService` | CRUD menu, cây menu |
| `UserService` | CRUD user, gán role |
| `AttendanceService` | Chấm công, tính OT |
| `LeaveService` | Đơn nghỉ phép, phê duyệt |
| `SalaryService` | Tính lương, BHXH, thuế TNCN |
| `ContractService` | Hợp đồng lao động |
| `AdvanceService` | Tạm ứng lương |
| `EmployeeEventService` | Biến động nhân sự |
| `ReportService` | Báo cáo tổng hợp |
| `AuditService` | Nhật ký hệ thống |
| `BackupService` | Sao lưu/phục hồi DB |
| `CompanySettingsService` | Thiết lập công ty |

### Repositories (16 repositories)

`BaseRepository<T>` (generic CRUD) + 15 repository chuyên biệt, tất cả sử dụng **Dapper** micro-ORM.

### Entities (15 models)

`Employee`, `Department`, `Position`, `User`, `Role`, `MenuNode`, `Category`, `CompanySetting`, `AttendanceRecord`, `LeaveRequest`, `SalaryRecord`, `Contract`, `Advance`, `EmployeeEvent`, `AuditLog`
