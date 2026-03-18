# 📊 BÁO CÁO TÍNH NĂNG - Quản Lý Nhân Viên

**Ngày cập nhật:** 18/03/2026  
**Tổng tính năng:** 64 | **✅ Đã làm:** 55 (~86%) | **⚠️ Một phần:** 4 | **❌ Chưa làm:** 5

---

## 1. Menu Hệ Thống (System) — ✅ 11/11 (100%)

| # | Tính năng | Trạng thái | File / Ghi chú |
|:-:|-----------|:---:|----------------|
| 1 | Đăng nhập | ✅ | `FrmLogin.cs` — BCrypt hash, khóa tài khoản |
| 2 | Đăng xuất | ✅ | Nút trong `FrmMain.cs` |
| 3 | Đổi mật khẩu | ✅ | `FrmChangePassword.cs` + nút "🔑 Đổi MK" |
| 4 | Quản lý người dùng (CRUD) | ✅ | `FrmUserManager.cs` + `UserService.cs` |
| 5 | Phân quyền - Quản lý vai trò | ✅ | `FrmRoleList.cs` |
| 6 | Phân quyền - Gán quyền theo menu | ✅ | `FrmRolePermission.cs` |
| 7 | Phân quyền - Gán role cho user | ✅ | `FrmUserDetail` — CheckedListBox roles |
| 8 | Quản lý Menu | ✅ | `FrmMenuManager.cs` |
| 9 | Nhật ký hệ thống (Audit Log) | ✅ | `FrmAuditLog.cs` |
| 10 | Sao lưu & Phục hồi DB | ✅ | `FrmBackupRestore.cs` + `BackupService.cs` |
| 11 | Thiết lập chung (công ty, logo) | ✅ | `FrmCompanySettings.cs` |

---

## 2. Menu Danh Mục (Categories) — ✅ 6/6 (100%)

| # | Tính năng | Trạng thái | File / Ghi chú |
|:-:|-----------|:---:|----------------|
| 1 | Phòng ban / Tổ nhóm | ✅ | `FrmDepartmentList.cs` |
| 2 | Chức vụ / Chức danh | ✅ | `FrmPositionList.cs` |
| 3 | Trình độ học vấn | ✅ | Seed data `EDUCATION_LEVEL` (6 items) |
| 4 | Loại hợp đồng | ✅ | Seed data `CONTRACT_TYPE` (4 items) |
| 5 | Ca làm việc | ✅ | Seed data `SHIFT_TYPE` |
| 6 | Quản lý danh mục chung | ✅ | `FrmCategoryManager.cs` |

---

## 3. Menu Nhân Sự (HR) — ✅ 10/12 (83%)

| # | Tính năng | Trạng thái | File / Ghi chú |
|:-:|-----------|:---:|----------------|
| 1 | Danh sách NV (lọc, tìm kiếm) | ✅ | `FrmEmployeeList.cs` |
| 2 | Thêm / Sửa hồ sơ | ✅ | `FrmEmployeeDetail.cs` (4 tabs) |
| 3 | Xóa hồ sơ (soft delete) | ✅ | |
| 4 | Xóa hàng loạt (Batch Delete) | ✅ | Checkbox multi-select + batch delete |
| 5 | Import Excel | ✅ | ClosedXML — nhập từ .xlsx |
| 6 | Export Excel | ✅ | ClosedXML — xuất danh sách .xlsx |
| 7 | Thông tin cá nhân + ngân hàng | ✅ | Tab Thông tin chung + Tài chính |
| 8 | Người phụ thuộc | ✅ | `NumberOfDependents` — NumericUpDown tab Tài chính |
| 9 | Hợp đồng lao động | ✅ | `FrmContractManager.cs` — CRUD + cảnh báo HĐ sắp hết hạn |
| 10 | Biến động nhân sự | ✅ | `FrmEmployeeEvents.cs` — Khen/Kỷ luật/Thăng chức/Điều chuyển |
| 11 | Sửa hàng loạt (Batch Edit) | ❌ | Chưa có UI multi-select batch edit fields |
| 12 | Gửi phiếu lương qua email | ❌ | Cần tích hợp SMTP |

---

## 4. Menu Chấm Công (Timekeeping) — ✅ 5/7 (71%)

| # | Tính năng | Trạng thái | File / Ghi chú |
|:-:|-----------|:---:|----------------|
| 1 | Chấm công ngày (giờ vào/ra) | ✅ | `FrmAttendanceDaily.cs` |
| 2 | Quản lý nghỉ phép | ✅ | `FrmLeaveRequest.cs` |
| 3 | Bảng công tổng hợp tháng | ✅ | `FrmAttendanceMonthly.cs` |
| 4 | Tính giờ OT | ✅ | `OvertimeHours` → tính lương |
| 5 | Chấm công hàng loạt | ⚠️ | Chấm theo phòng ban/ngày, chưa batch nhiều NV 1 lúc |
| 6 | Tự động phát hiện đi muộn/về sớm | ⚠️ | Có status "Late" nhưng chưa tự động so sánh với ca |
| 7 | Kết nối máy chấm công | ❌ | Cần SDK thiết bị cụ thể |

---

## 5. Menu Tính Lương (Payroll) — ✅ 12/14 (86%)

| # | Tính năng | Trạng thái | File / Ghi chú |
|:-:|-----------|:---:|----------------|
| 1 | Thiết lập công thức & định mức | ✅ | `FrmSalaryConfig.cs` |
| 2 | Hệ số lương | ✅ | `SalaryCoefficient` |
| 3 | Phụ cấp chức vụ | ✅ | `AllowanceAmount` từ Position |
| 4 | BHXH / BHYT / BHTN | ✅ | Tính tự động % |
| 5 | Thuế TNCN lũy tiến 7 bậc | ✅ | `CalculatePIT()` — đúng biểu thuế VN |
| 6 | Giảm trừ bản thân + NPT | ✅ | 11tr + 4.4tr/NPT |
| 7 | Lập bảng lương tháng tự động | ✅ | `CalculateMonthlyAsync()` |
| 8 | Quản lý tạm ứng | ✅ | `FrmAdvanceManager.cs` + `AdvanceService.cs` |
| 9 | In / Xuất phiếu lương | ✅ | `FrmPayslip.cs` |
| 10 | Khóa bảng lương (Approved) | ✅ | `ApproveAllAsync()` |
| 11 | Lịch sử lương | ✅ | `FrmSalaryHistory.cs` |
| 12 | Phụ cấp khác (ăn trưa, đi lại) | ⚠️ | `OtherAllowance = 0` cứng, cần UI config |
| 13 | Khấu trừ (phạt đi muộn) | ⚠️ | `OtherDeductions = 0` cứng, cần UI config |
| 14 | Gửi phiếu lương qua email | ❌ | Cần SMTP integration |

---

## 6. Menu Báo Cáo & Dashboard — ✅ 12/14 (86%)

| # | Tính năng | Trạng thái | File / Ghi chú |
|:-:|-----------|:---:|----------------|
| 1 | Dashboard KPI cards | ✅ | `FrmDashboard.cs` |
| 2 | Biểu đồ NV theo phòng ban | ✅ | Bar chart custom |
| 3 | Biểu đồ chi lương theo tháng | ✅ | Bar chart 12 tháng |
| 4 | Top 5 lương cao nhất | ✅ | Table ranking |
| 5 | Báo cáo danh sách NV | ✅ | `FrmReportExport.cs` |
| 6 | Báo cáo NV mới / nghỉ việc | ✅ | Report type "NV mới / nghỉ việc" |
| 7 | Báo cáo sinh nhật trong tháng | ✅ | Report type "Sinh nhật tháng" |
| 8 | Báo cáo bảng lương chi tiết | ✅ | Report type "Bảng lương tháng" |
| 9 | Báo cáo BHXH | ✅ | Report type "Bảo hiểm xã hội" |
| 10 | Báo cáo thuế TNCN | ✅ | Report type "Thuế TNCN" |
| 11 | Biến động nhân sự (tỷ lệ vào/ra) | ✅ | Report type "Biến động nhân sự" |
| 12 | Xuất CSV + Excel | ✅ | CSV + ClosedXML (.xlsx) |
| 13 | In báo cáo | ✅ | PrintPreview |
| 14 | Báo cáo chấm công (đi muộn thường xuyên) | ❌ | Có chấm công tháng nhưng chưa phân tích đi muộn chi tiết |

---

## 📈 Tổng Kết

| Menu | ✅ | ⚠️ | ❌ | Tổng | % |
|------|:--:|:--:|:--:|:----:|:-:|
| Hệ thống | 11 | 0 | 0 | 11 | 100% |
| Danh mục | 6 | 0 | 0 | 6 | 100% |
| Nhân sự | 10 | 0 | 2 | 12 | 83% |
| Chấm công | 4 | 2 | 1 | 7 | 71% |
| Tính lương | 11 | 2 | 1 | 14 | 86% |
| Báo cáo | 13 | 0 | 1 | 14 | 93% |
| **TỔNG** | **55** | **4** | **5** | **64** | **86%** |

---

## ❌ Tính năng chưa làm (5)

| # | Tính năng | Lý do / Ghi chú |
|:-:|-----------|------------------|
| 1 | Sửa hàng loạt (Batch Edit) | Cần thiết kế UI phức tạp cho multi-field edit |
| 2 | Gửi phiếu lương qua email | Cần SMTP server config + template email |
| 3 | Kết nối máy chấm công | Phụ thuộc SDK thiết bị cụ thể (ZKTeco, v.v.) |
| 4 | Gửi email (chung) | Cần tích hợp SMTP |
| 5 | Phân tích đi muộn thường xuyên | Cần logic so sánh CheckIn vs CaLamViec |

## ⚠️ Tính năng một phần (4)

| # | Tính năng | Cần bổ sung |
|:-:|-----------|-------------|
| 1 | Chấm công hàng loạt | Thêm UI batch cho nhiều NV cùng lúc |
| 2 | Tự động phát hiện đi muộn/về sớm | So sánh CheckIn/CheckOut với giờ ca |
| 3 | Phụ cấp khác | UI config phụ cấp ăn trưa, đi lại, xăng xe |
| 4 | Khấu trừ phạt đi muộn | UI config quy tắc khấu trừ |
