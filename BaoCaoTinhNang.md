# 📊 BÁO CÁO TÍNH NĂNG CHI TIẾT — Quản Lý Nhân Viên

**Ngày cập nhật:** 19/03/2026  
**Tổng tính năng:** 72 | **✅ Hoàn thành:** 71 (~99%) | **❌ Chưa làm:** 1 (phần cứng)

---

## 📑 Mục Lục

1. [Menu Hệ Thống (System)](#1-menu-hệ-thống-system--1111-100)
2. [Menu Danh Mục (Categories)](#2-menu-danh-mục-categories--77-100)
3. [Menu Nhân Sự (HR)](#3-menu-nhân-sự-hr--1414-100)
4. [Menu Chấm Công (Timekeeping)](#4-menu-chấm-công-timekeeping--89-89)
5. [Menu Tính Lương (Payroll)](#5-menu-tính-lương-payroll--1616-100)
6. [Menu Báo Cáo & Dashboard](#6-menu-báo-cáo--dashboard--1515-100)
7. [Cải thiện Kiến trúc](#7-cải-thiện-kiến-trúc---66-100)
8. [Unit Tests](#8-unit-tests)
9. [Tổng Kết](#-tổng-kết)

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

## 2. Menu Danh Mục (Categories) — ✅ 7/7 (100%)

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 1 | Phòng ban / Tổ nhóm | ✅ | `FrmDepartmentList.cs`, `DepartmentService.cs` | CRUD phòng ban, soft delete, kiểm tra nhân viên thuộc phòng ban trước khi xóa |
| 2 | Chức vụ / Chức danh | ✅ | `FrmPositionList.cs`, `PositionService.cs` | CRUD chức vụ, cấp bậc (Level), phụ cấp chức vụ (`AllowanceAmount`) |
| 3 | Trình độ học vấn | ✅ | Seed data `EDUCATION_LEVEL` | 6 lựa chọn: Sau đại học, Đại học, Cao đẳng, Trung cấp, THPT, Khác |
| 4 | Loại hợp đồng | ✅ | Seed data `CONTRACT_TYPE` | 4 loại: Thử việc, Có thời hạn, Không thời hạn, Thời vụ |
| 5 | Ca làm việc | ✅ | Seed data `SHIFT_TYPE` | Ca sáng, ca chiều, ca đêm, hành chính |
| 6 | Quản lý danh mục chung | ✅ | `FrmCategoryManager.cs`, `CategoryService.cs` | Giao diện quản lý chung cho tất cả danh mục, hỗ trợ cache 3 tầng (Memory → JSON → DB) |
| 7 | **🆕 Quản lý ngày lễ** | ✅ | `FrmHolidayManager.cs`, `HolidayRepository.cs` | CRUD ngày lễ từ DB (thay hardcode), filter theo năm, checkbox lặp hàng năm, seed Tết + ngày lễ VN 2026 |

**🔹 Cơ chế Cache 3 tầng:**
```
Đọc danh mục → Memory Cache (nhanh nhất)
    ↓ miss
→ JSON File Cache (Data/categories.json)
    ↓ miss
→ Database (SQL Server)
    ↑ cập nhật ngược lại cache
```

---

## 3. Menu Nhân Sự (HR) — ✅ 14/14 (100%)

### 3.1 Quản lý Nhân viên

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 1 | Danh sách NV (lọc, tìm kiếm) | ✅ | `FrmEmployeeList.cs`, `EmployeeService.cs` | DataGridView, tìm kiếm theo tên/mã NV, lọc theo phòng ban, server-side pagination |
| 2 | Thêm / Sửa hồ sơ | ✅ | `FrmEmployeeDetail.cs` | Form 4 tabs: Thông tin chung, Liên hệ, Tài chính, Ghi chú. Validate đầy đủ (CCCD, email, SĐT) |
| 3 | Xóa hồ sơ (soft delete) | ✅ | `EmployeeService.cs` | Đánh dấu `IsActive = false`, không xóa vật lý, giữ lại để tra cứu |
| 4 | Xóa hàng loạt (Batch Delete) | ✅ | `FrmEmployeeList.cs` | Checkbox multi-select rows + nút xóa hàng loạt, xác nhận trước khi xóa |
| 5 | Import Excel | ✅ | `FrmEmployeeList.cs` + ClosedXML | Nhập danh sách từ file `.xlsx`, validate dữ liệu dòng, báo lỗi chi tiết |
| 6 | Export Excel | ✅ | `FrmEmployeeList.cs` + ClosedXML | Xuất danh sách nhân viên ra `.xlsx`, tùy chọn cột |

### 3.2 Thông tin Chi tiết

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 7 | Thông tin cá nhân + ngân hàng | ✅ | `FrmEmployeeDetail.cs` (Tab 1 + Tab 3) | Họ tên, ngày sinh, CCCD, địa chỉ, SĐT, email, số tài khoản, ngân hàng |
| 8 | Người phụ thuộc | ✅ | `FrmEmployeeDetail.cs` (Tab Tài chính) | Trường `NumberOfDependents` — dùng giảm trừ thuế TNCN (4.4 triệu/NPT) |
| 9 | Hợp đồng lao động | ✅ | `FrmContractManager.cs`, `ContractService.cs` | CRUD hợp đồng, cảnh báo hết hạn (30 ngày), **mức lương trên hợp đồng** (`ContractSalary`) |
| 10 | Biến động nhân sự | ✅ | `FrmEmployeeEvents.cs`, `EmployeeEventService.cs` | Ghi nhận sự kiện: Khen thưởng, Kỷ luật, Thăng chức, Điều chuyển — lịch sử đầy đủ |
| 11 | Sửa hàng loạt (Batch Edit) | ✅ | `FrmEmployeeList.cs`, `EmployeeService.cs` | Dialog multi-select rows + chọn trường (Phòng ban, Chức vụ, Hệ số lương, NPT) + preview |
| 12 | Gửi phiếu lương qua email | ✅ | `EmailService.cs`, `FrmSalaryCalculation.cs` | SMTP config trong `FrmCompanySettings`, template HTML phiếu lương |

### 3.3 Tính năng mới (MISS-4)

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 13 | **🆕 Auto-deactivate NV hết HĐ** | ✅ | `ContractService.DeactivateExpiredEmployeesAsync()` | Tự động phát hiện NV có HĐ hết hạn + không có HĐ mới → deactivate NV |
| 14 | **🆕 Mức lương trên hợp đồng** | ✅ | `Contract.ContractSalary`, `ContractRepository.cs` | Trường `ContractSalary DECIMAL(18,2)` ghi trên HĐ, dùng đối chiếu với lương thực tế |

---

## 4. Menu Chấm Công (Timekeeping) — ✅ 8/9 (89%)

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 1 | Chấm công ngày (giờ vào/ra) | ✅ | `FrmAttendanceDaily.cs`, `AttendanceService.cs` | Nhập giờ CheckIn/CheckOut, tự động tính giờ làm việc, OT |
| 2 | Quản lý nghỉ phép | ✅ | `FrmLeaveRequest.cs`, `LeaveService.cs` | Tạo đơn nghỉ phép (Phép năm, Ốm, Thai sản, Không lương), phê duyệt/từ chối |
| 3 | **🆕 Giới hạn phép năm 12 ngày** | ✅ | `LeaveService.CreateAsync()` | Kiểm tra số ngày phép đã dùng trước khi tạo đơn, theo Điều 113 Bộ Luật LĐ VN |
| 4 | **🆕 Auto chấm công khi duyệt phép** | ✅ | `LeaveService.ApproveAsync()` | Tự động tạo `AttendanceRecord` (OnLeave/UnpaidLeave) cho các ngày nghỉ đã duyệt, bỏ qua T7/CN/ngày lễ |
| 5 | Bảng công tổng hợp tháng | ✅ | `FrmAttendanceMonthly.cs` | Tổng hợp ngày công, ngày nghỉ, giờ OT theo tháng — dữ liệu đầu vào cho tính lương |
| 6 | Tính giờ OT | ✅ | `AttendanceService.cs` → `SalaryService.cs` | `OvertimeHours` được tính từ chấm công, nhân hệ số 1.5x/2x khi tính lương |
| 7 | Chấm công hàng loạt | ✅ | `FrmAttendanceDaily.cs` → `DlgBulkAttendance` | DataGridView inline edit: chọn NV, nhập giờ vào/ra, tự động detect status |
| 8 | Tự động phát hiện đi muộn/về sớm | ✅ | `AttendanceService.DetermineStatus()` | So sánh CheckIn/CheckOut với giờ ca (±15 phút), trả về `Late`/`EarlyLeave`/`LateAndEarlyLeave` |
| 9 | Kết nối máy chấm công | ❌ | — | Phụ thuộc SDK thiết bị cụ thể (ZKTeco, Suprema), sẽ làm khi vào dự án thực tế |

---

## 5. Menu Tính Lương (Payroll) — ✅ 16/16 (100%)

### 5.1 Cấu hình & Công thức

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 1 | Thiết lập công thức & định mức | ✅ | `FrmSalaryConfig.cs`, `SalaryService.cs` | Cấu hình lương cơ sở, ngày công chuẩn, tỉ lệ BHXH/BHYT/BHTN, **lọc theo ngày hiệu lực** |
| 2 | Hệ số lương | ✅ | `Employee.SalaryCoefficient` | Mỗi nhân viên có hệ số riêng, nhân với lương cơ sở |
| 3 | Phụ cấp chức vụ | ✅ | `Position.AllowanceAmount` | Phụ cấp tự động theo chức vụ hiện tại |

### 5.2 Tính toán

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 4 | BHXH / BHYT / BHTN | ✅ | `SalaryService.CalculateForEmployeeAsync()` | Tính tự động: BHXH 8%, BHYT 1.5%, BHTN 1% (phần NLĐ) |
| 5 | Thuế TNCN lũy tiến 7 bậc | ✅ | `SalaryService.CalculatePIT()` | Đúng biểu thuế VN: 5%, 10%, 15%, 20%, 25%, 30%, 35% — **có unit test** |
| 6 | Giảm trừ bản thân + NPT | ✅ | `SalaryService.cs` | Giảm trừ 11 triệu/bản thân + 4.4 triệu/người phụ thuộc |
| 7 | Lập bảng lương tháng tự động | ✅ | `SalaryService.CalculateMonthlyAsync()` | Tính lương hàng loạt, **batch load configs** (1 query thay 12 query/NV) |
| 8 | **🆕 Trừ tạm ứng vào lương** | ✅ | `SalaryService.cs`, `AdvanceRepository.cs` | Tự động truy xuất tổng tạm ứng đã duyệt và trừ vào lương thực nhận |

### 5.3 Quản lý & Xuất

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 9 | Quản lý tạm ứng | ✅ | `FrmAdvanceManager.cs`, `AdvanceService.cs` | Tạo phiếu tạm ứng, duyệt, trừ vào lương tháng |
| 10 | In / Xuất phiếu lương | ✅ | `FrmPayslip.cs` | Phiếu lương chi tiết từng NV, hỗ trợ in & export |
| 11 | Khóa bảng lương (Approved) | ✅ | `SalaryService.ApproveAllAsync()` | Khóa bảng lương sau khi duyệt, ngăn chỉnh sửa |
| 12 | **🆕 Hủy duyệt phiếu lương** | ✅ | `SalaryService.UnapproveAsync()` | Chuyển phiếu lương từ Approved → Draft, cho phép tính lại |
| 13 | Lịch sử lương | ✅ | `FrmSalaryHistory.cs` | Tra cứu lương theo NV & khoảng thời gian |
| 14 | Phụ cấp khác (ăn trưa, đi lại) | ✅ | `SalaryService.cs` | Lấy từ SalaryConfigs: `PC_ANTRUOI`, `PC_XANGXE`, `PC_DILAI` |
| 15 | Khấu trừ (phạt đi muộn) | ✅ | `SalaryService.cs` | Config `PHAT_DIMUON_MUC` × (lần muộn − ngưỡng), hỗ trợ `LateAndEarlyLeave` (tính 2 lần) |
| 16 | Gửi phiếu lương qua email | ✅ | `EmailService.cs`, `FrmSalaryCalculation.cs` | SMTP config + template HTML + gửi từ bảng lương |

**🔹 Công thức tính lương:**
```
Lương thực nhận = Lương cơ bản × Hệ số × (Ngày công / Ngày chuẩn)
                + Phụ cấp chức vụ
                + Phụ cấp khác (ăn trưa, xăng xe, đi lại)
                + Lương OT (giờ OT × hệ số OT × lương giờ)
                - BHXH (8%) - BHYT (1.5%) - BHTN (1%)
                - Thuế TNCN (7 bậc lũy tiến)
                - Tạm ứng đã duyệt
                - Phạt đi muộn/về sớm
```

---

## 6. Menu Báo Cáo & Dashboard — ✅ 15/15 (100%)

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
| 5 | Báo cáo danh sách NV | ✅ | `FrmReportExport.cs`, `ReportService.cs` | Danh sách toàn bộ nhân viên, lọc theo phòng ban/trạng thái |
| 6 | Báo cáo NV mới / nghỉ việc | ✅ | `FrmReportExport.cs` | Báo cáo nhân viên vào/ra theo khoảng thời gian |
| 7 | Báo cáo sinh nhật trong tháng | ✅ | `FrmReportExport.cs` | Danh sách NV có sinh nhật trong tháng được chọn |
| 8 | Báo cáo bảng lương chi tiết | ✅ | `FrmReportExport.cs` | Chi tiết lương từng NV theo tháng: lương gộp, BHXH, thuế, thực nhận |
| 9 | Báo cáo BHXH (phần NLĐ) | ✅ | `ReportRepository.GetInsuranceReportAsync()` | Tổng hợp BHXH/BHYT/BHTN phần người lao động |
| 10 | **🆕 Báo cáo BHXH (phần DN)** | ✅ | `ReportRepository.GetEmployerInsuranceReportAsync()` | BHXH 17.5% + BHYT 3% + BHTN 1% = 21.5% phần doanh nghiệp, tổng hợp cả 2 bên |
| 11 | Báo cáo thuế TNCN | ✅ | `ReportRepository.GetTaxReportAsync()` | Chi tiết thuế TNCN từng NV: thu nhập chịu thuế, giảm trừ, thuế phải nộp |
| 12 | Biến động nhân sự | ✅ | `ReportRepository.GetTurnoverAsync()` | Tỉ lệ vào/ra, thống kê biến động 12 tháng trong năm |
| 13 | Báo cáo đi muộn thường xuyên | ✅ | `ReportRepository.GetLateFrequencyReportAsync()` | Thống kê tần suất đi muộn/về sớm, tỉ lệ vi phạm, tô màu cảnh báo |

### 6.3 Xuất & In

| # | Tính năng | Trạng thái | File liên quan | Chi tiết |
|:-:|-----------|:---:|----------------|---------|
| 14 | Xuất CSV + Excel | ✅ | `FrmReportExport.cs` + ClosedXML | Xuất báo cáo ra `.csv` hoặc `.xlsx`, tự động format |
| 15 | In báo cáo | ✅ | `FrmReportExport.cs` | PrintPreview + Print, hỗ trợ thiết lập trang |

---

## 7. Cải thiện Kiến trúc — ✅ 6/6 (100%)

| # | Cải thiện | File liên quan | Chi tiết |
|:-:|-----------|----------------|---------|
| 1 | **Service Interfaces** | `IServices.cs` | 4 interfaces: `ISalaryService`, `ILeaveService`, `IAttendanceService`, `IContractService` — hỗ trợ DI & Moq testing |
| 2 | **Batch Config Query** | `SalaryRepository.GetConfigDictionaryAsync()` | Load tất cả configs trong 1 query thay vì 12 query riêng lẻ, giảm ~1100 queries khi tính lương 100 NV |
| 3 | **Guard Clauses** | `Guard.cs` | Utility validation nhẹ: `NotNullOrEmpty`, `PositiveId`, `NonNegative`, `ValidDateRange`, `ValidMonth` |
| 4 | **StatusConstants** | `StatusConstants.cs` | Loại bỏ tất cả magic strings (`"Present"`, `"Late"`, `"Approved"`) thay bằng hằng số typed — áp dụng trong 3 services |
| 5 | **Result Pattern** | `Result.cs`, `Result<T>` | Pattern thống nhất thay tuple `(bool, string)`, sẵn sàng sử dụng cho features mới |
| 6 | **InternalsVisibleTo** | `QuanLyNhanVien.BLL.csproj` | Cho phép test project truy cập `internal` methods (CalculatePIT) |

---

## 8. Unit Tests

**Project:** `QuanLyNhanVien.Tests` (xUnit + Moq)

| Test Class | Số tests | Mô tả |
|------------|:--------:|-------|
| `SalaryServiceTests` | 8 | Kiểm tra `CalculatePIT()` — tất cả 7 bậc thuế TNCN, zero/negative income, rounding |

```
✅ CalculatePIT_ZeroIncome_ReturnsZero
✅ CalculatePIT_NegativeIncome_ReturnsZero
✅ CalculatePIT_Bracket1_5Percent (1M → 50K, 5M → 250K)
✅ CalculatePIT_Bracket2_10Percent (7M → 450K, 10M → 750K)
✅ CalculatePIT_HigherBrackets (18M/32M/52M/80M)
✅ CalculatePIT_Bracket7_35Percent (100M → 25.15M)
✅ CalculatePIT_SmallAmount_CorrectRounding
```

---

## 📈 Tổng Kết

| Menu | ✅ Hoàn thành | ❌ Chưa làm | Tổng | Tỉ lệ |
|------|:-----------:|:----------:|:----:|:-----:|
| 🔧 Hệ thống | 11 | 0 | 11 | **100%** |
| 📂 Danh mục | 7 | 0 | 7 | **100%** |
| 👥 Nhân sự | 14 | 0 | 14 | **100%** |
| ⏰ Chấm công | 8 | 1 | 9 | **89%** |
| 💰 Tính lương | 16 | 0 | 16 | **100%** |
| 📊 Báo cáo | 15 | 0 | 15 | **100%** |
| **TỔNG CỘNG** | **71** | **1** | **72** | **99%** |

### Biểu đồ tiến độ

```
Hệ thống   ████████████████████ 100%  (11/11)
Danh mục   ████████████████████ 100%  (7/7)
Nhân sự    ████████████████████ 100%  (14/14)
Tính lương ████████████████████ 100%  (16/16)
Báo cáo    ████████████████████ 100%  (15/15)
Chấm công  ██████████████████░░  89%  (8/9)
```

---

## ❌ Tính Năng Chưa Làm (1)

| # | Tính năng | Module | Ưu tiên | Lý do / Phụ thuộc |
|:-:|-----------|--------|:-------:|-------------------|
| 1 | Kết nối máy chấm công | Chấm công | Thấp | Phụ thuộc SDK thiết bị cụ thể (ZKTeco, Suprema, HikVision...), sẽ tích hợp khi vào dự án thực tế |

---



## 📊 Thống Kê Code

### Forms (28 forms)

| Thư mục | Số form | Forms |
|---------|:-------:|-------|
| `Main` | 2 | FrmLogin, FrmMain |
| `Employee` | 4 | FrmEmployeeList, FrmEmployeeDetail, FrmContractManager, FrmEmployeeEvents |
| `Department` | 1 | FrmDepartmentList |
| `Position` | 1 | FrmPositionList |
| `Category` | 2 | FrmCategoryManager, **FrmHolidayManager** |
| `Permission` | 3 | FrmMenuManager, FrmRoleList, FrmRolePermission |
| `Attendance` | 3 | FrmAttendanceDaily, FrmAttendanceMonthly, FrmLeaveRequest |
| `Salary` | 5 | FrmSalaryCalculation, FrmSalaryConfig, FrmSalaryHistory, FrmPayslip, FrmAdvanceManager |
| `Report` | 2 | FrmDashboard, FrmReportExport |
| `System` | 5 | FrmUserManager, FrmAuditLog, FrmBackupRestore, FrmChangePassword, FrmCompanySettings |

### Services (18 services + 4 interfaces)

| Service | Chức năng chính | Interface |
|---------|----------------|-----------|
| `AuthService` | Xác thực, BCrypt hash, khóa tài khoản | — |
| `EmployeeService` | CRUD nhân viên, pagination, search | — |
| `SalaryService` | Tính lương, BHXH, thuế TNCN, batch config | `ISalaryService` |
| `AttendanceService` | Chấm công, tính OT, `DetermineStatus` | `IAttendanceService` |
| `LeaveService` | Đơn nghỉ phép, phê duyệt, auto chấm công | `ILeaveService` |
| `ContractService` | Hợp đồng, auto-deactivate hết hạn | `IContractService` |
| `ReportService` | Báo cáo tổng hợp, BHXH DN | — |
| `AdvanceService` | Tạm ứng lương | — |
| `CategoryService` | Danh mục + cache 3 tầng | — |
| `+ 9 services khác` | Menu, User, Backup, Email, Audit... | — |

### Repositories (17 repositories)

`BaseRepository<T>` (generic CRUD) + 16 repository chuyên biệt + `HolidayRepository` (mới), tất cả sử dụng **Dapper** micro-ORM.

### Entities (17 models)

`Employee`, `Department`, `Position`, `User`, `Role`, `MenuNode`, `Category`, `CompanySetting`, `AttendanceRecord`, `LeaveRequest`, `SalaryRecord`, `SalaryConfig`, `Contract`, `Advance`, `EmployeeEvent`, `AuditLog`, **`Holiday`**

### Common (3 files mới)

`Result.cs`, `StatusConstants.cs`, `Guard.cs` — trong `QuanLyNhanVien.Models/Common/`

### Database Migrations (14 files)

`01_CreateDatabase.sql` → ... → `11_SampleData.sql` + `12_SalaryConfigFix.sql` + `13_BugFixes_AdvanceHoliday.sql` + `14_MissFeatures.sql`
