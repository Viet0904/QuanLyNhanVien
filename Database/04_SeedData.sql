-- =============================================
-- Quản Lý Nhân Viên - Seed Data
-- =============================================
USE QuanLyNhanVien;
GO

-- =============================================
-- 1. Roles
-- =============================================
INSERT INTO Roles (RoleName, [Description], IsSystem) VALUES
(N'Administrator', N'Quản trị viên hệ thống - toàn quyền', 1),
(N'HR Manager', N'Quản lý nhân sự', 0),
(N'Accountant', N'Kế toán - quản lý lương', 0),
(N'Viewer', N'Chỉ xem', 0);
GO

-- =============================================
-- 2. Admin User (default credentials — see README)
-- BCrypt hash
-- =============================================
INSERT INTO Users (Username, PasswordHash, Salt, IsActive) VALUES
(N'admin', N'$2a$12$LJ3m4ys3GzW./cGBgdVsaOWzBhL4LGE34HEYep6Ljs4.AvE3FKXW.', N'', 1);
GO

-- Gán role Admin cho user admin
INSERT INTO UserRoles (UserId, RoleId) VALUES (1, 1);
GO

-- =============================================
-- 3. Departments (Phòng ban)
-- =============================================
INSERT INTO Departments (DepartmentCode, DepartmentName) VALUES
(N'BGD', N'Ban Giám Đốc'),
(N'NS', N'Phòng Nhân Sự'),
(N'KT', N'Phòng Kế Toán'),
(N'CNTT', N'Phòng Công Nghệ Thông Tin'),
(N'KD', N'Phòng Kinh Doanh'),
(N'MKT', N'Phòng Marketing'),
(N'HC', N'Phòng Hành Chính');
GO

-- =============================================
-- 4. Positions (Chức vụ)
-- =============================================
INSERT INTO Positions (PositionName, [Level], AllowanceAmount) VALUES
(N'Giám đốc', 10, 10000000),
(N'Phó Giám đốc', 9, 8000000),
(N'Trưởng phòng', 7, 5000000),
(N'Phó phòng', 6, 3000000),
(N'Nhóm trưởng', 5, 2000000),
(N'Chuyên viên', 3, 1000000),
(N'Nhân viên', 1, 0),
(N'Thực tập sinh', 0, 0);
GO

-- =============================================
-- 5. Menus (Hệ thống menu)
-- =============================================
-- Level 0: Root groups
INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder) VALUES
(N'NHANSU', N'Nhân Sự', NULL, NULL, N'people', 1),
(N'CHAMCONG', N'Chấm Công', NULL, NULL, N'clock', 2),
(N'LUONG', N'Lương', NULL, NULL, N'money', 3),
(N'BAOCAO', N'Báo Cáo', NULL, NULL, N'chart', 4),
(N'HETHONG', N'Hệ Thống', NULL, NULL, N'settings', 5);
GO

-- Level 1: Sub-menus
-- Nhân sự
INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder) VALUES
(N'NS_DSNV', N'Danh Sách Nhân Viên', 1, N'FrmEmployeeList', N'user', 1),
(N'NS_PB', N'Phòng Ban', 1, N'FrmDepartmentList', N'organization', 2),
(N'NS_CV', N'Chức Vụ', 1, N'FrmPositionList', N'badge', 3);
GO

-- Chấm công
INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder) VALUES
(N'CC_NGAY', N'Chấm Công Ngày', 2, N'FrmAttendanceDaily', N'calendar-check', 1),
(N'CC_THANG', N'Bảng Chấm Công Tháng', 2, N'FrmAttendanceMonthly', N'calendar', 2),
(N'CC_PHEP', N'Đơn Xin Nghỉ Phép', 2, N'FrmLeaveRequest', N'document', 3);
GO

-- Lương
INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder) VALUES
(N'LG_CAUHINH', N'Cấu Hình Lương', 3, N'FrmSalaryConfig', N'settings', 1),
(N'LG_TINH', N'Tính Lương Tháng', 3, N'FrmSalaryCalculation', N'calculator', 2),
(N'LG_PHIEU', N'Phiếu Lương', 3, N'FrmSalarySlip', N'receipt', 3),
(N'LG_LICHSU', N'Lịch Sử Lương', 3, N'FrmSalaryHistory', N'history', 4);
GO

-- Báo cáo
INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder) VALUES
(N'BC_DASHBOARD', N'Dashboard', 4, N'FrmDashboard', N'dashboard', 1),
(N'BC_VIEWER', N'Xem Báo Cáo', 4, N'FrmReportViewer', N'report', 2);
GO

-- Hệ thống
INSERT INTO Menus (MenuCode, MenuName, ParentId, FormName, IconName, SortOrder) VALUES
(N'HT_VAITRO', N'Quản Lý Vai Trò', 5, N'FrmRoleList', N'shield', 1),
(N'HT_PHANQUYEN', N'Phân Quyền', 5, N'FrmRolePermission', N'key', 2),
(N'HT_MENU', N'Quản Lý Menu', 5, N'FrmMenuManager', N'menu', 3),
(N'HT_DANHMUC', N'Danh Mục', 5, N'FrmCategoryManager', N'list', 4),
(N'HT_LOG', N'Nhật Ký Thao Tác', 5, N'FrmAuditLog', N'log', 5);
GO

-- =============================================
-- 6. Admin Full Permissions (tất cả menu)
-- =============================================
INSERT INTO RolePermissions (RoleId, MenuId, CanView, CanAdd, CanEdit, CanDelete, CanExport, CanPrint)
SELECT 1, MenuId, 1, 1, 1, 1, 1, 1 FROM Menus;
GO

-- =============================================
-- 7. Categories (Danh mục)
-- =============================================
INSERT INTO Categories (CategoryCode, CategoryName, [Description], IsSystem) VALUES
(N'SHIFT_TYPE', N'Ca Làm Việc', N'Các ca làm việc trong ngày', 1),
(N'LEAVE_TYPE', N'Loại Nghỉ Phép', N'Các loại nghỉ phép', 1),
(N'GENDER', N'Giới Tính', N'Giới tính', 1),
(N'BANK', N'Ngân Hàng', N'Danh sách ngân hàng', 0);
GO

INSERT INTO CategoryItems (CategoryId, ItemCode, ItemName, ItemValue, SortOrder) VALUES
-- Ca làm việc
(1, N'MORNING', N'Ca Sáng', N'07:30-11:30', 1),
(1, N'AFTERNOON', N'Ca Chiều', N'13:00-17:00', 2),
(1, N'FULLDAY', N'Cả Ngày', N'07:30-17:00', 3),
(1, N'NIGHT', N'Ca Đêm', N'22:00-06:00', 4),
-- Loại nghỉ phép
(2, N'ANNUAL', N'Nghỉ Phép Năm', NULL, 1),
(2, N'SICK', N'Nghỉ Ốm', NULL, 2),
(2, N'UNPAID', N'Nghỉ Không Lương', NULL, 3),
(2, N'MATERNITY', N'Nghỉ Thai Sản', NULL, 4),
(2, N'BEREAVEMENT', N'Nghỉ Tang', NULL, 5),
(2, N'OTHER', N'Khác', NULL, 6),
-- Giới tính
(3, N'MALE', N'Nam', NULL, 1),
(3, N'FEMALE', N'Nữ', NULL, 2),
(3, N'OTHER', N'Khác', NULL, 3),
-- Ngân hàng
(4, N'VCB', N'Vietcombank', NULL, 1),
(4, N'TCB', N'Techcombank', NULL, 2),
(4, N'MB', N'MB Bank', NULL, 3),
(4, N'BIDV', N'BIDV', NULL, 4),
(4, N'VTB', N'VietinBank', NULL, 5),
(4, N'ACB', N'ACB', NULL, 6),
(4, N'TPB', N'TPBank', NULL, 7),
(4, N'SHB', N'SHB', NULL, 8);
GO

-- =============================================
-- 8. SalaryConfigs (Cấu hình lương mặc định)
-- =============================================
INSERT INTO SalaryConfigs (ConfigCode, ConfigName, ConfigValue, ConfigType, EffectiveFrom) VALUES
(N'BHXH_RATE', N'Tỷ lệ BHXH (NV đóng)', 8.0000, N'Percent', '2024-01-01'),
(N'BHYT_RATE', N'Tỷ lệ BHYT (NV đóng)', 1.5000, N'Percent', '2024-01-01'),
(N'BHTN_RATE', N'Tỷ lệ BHTN (NV đóng)', 1.0000, N'Percent', '2024-01-01'),
(N'PERSONAL_DEDUCTION', N'Giảm trừ bản thân', 11000000.0000, N'Amount', '2024-01-01'),
(N'DEPENDENT_DEDUCTION', N'Giảm trừ người phụ thuộc', 4400000.0000, N'Amount', '2024-01-01'),
(N'STANDARD_WORK_DAYS', N'Số ngày công chuẩn', 26.0000, N'Days', '2024-01-01'),
(N'OT_RATE_WEEKDAY', N'Hệ số tăng ca ngày thường', 1.5000, N'Percent', '2024-01-01'),
(N'OT_RATE_WEEKEND', N'Hệ số tăng ca cuối tuần', 2.0000, N'Percent', '2024-01-01'),
(N'OT_RATE_HOLIDAY', N'Hệ số tăng ca ngày lễ', 3.0000, N'Percent', '2024-01-01');
GO

PRINT N'Seed data thành công!';
GO
