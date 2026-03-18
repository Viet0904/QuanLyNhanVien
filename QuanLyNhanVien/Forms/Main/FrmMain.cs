using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.DTOs;
using QuanLyNhanVien.Models.Enums;
using QuanLyNhanVien.Forms.Employee;
using QuanLyNhanVien.Forms.Department;
using QuanLyNhanVien.Forms.Position;
using QuanLyNhanVien.Forms.Permission;
using QuanLyNhanVien.Forms.Category;
using QuanLyNhanVien.Forms.Attendance;
using QuanLyNhanVien.Forms.Salary;
using QuanLyNhanVien.Forms.Report;
using QuanLyNhanVien.Forms.SystemAdmin;
using System.IO;

namespace QuanLyNhanVien.Forms.Main
{
    public class FrmMain : Form
    {
        private Panel panelMenu = null!;
        private Panel panelContent = null!;
        private Panel panelNavItems = null!;
        private Panel _navInnerPanel = null!;
        private Panel? _activeNavItem = null;
        private readonly Dictionary<string, string> _menuFormMap = new();
        private readonly Dictionary<string, Panel> _navPanels = new();

        public FrmMain()
        {
            InitializeComponent();
            this.Load += FrmMain_Load;
        }

        private void InitializeComponent()
        {
            this.Text = $"Quản Lý Nhân Sự - {AppSession.DisplayName}";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);
            this.DoubleBuffered = true;

            // ===== SIDEBAR (tối) =====
            panelMenu = new Panel
            {
                Dock = DockStyle.Left, Width = 260,
                BackColor = ThemeColors.SidebarBg,
                Padding = new Padding(0)
            };

            // --- Logo Area ---
            var panelLogo = new Panel
            {
                Dock = DockStyle.Top, Height = 65,
                BackColor = Color.Transparent,
                Padding = new Padding(24, 18, 24, 18)
            };

            var iconBox = new Panel
            {
                Size = new Size(32, 32), Location = new Point(24, 16),
                BackColor = ThemeColors.Primary
            };
            iconBox.Paint += (s, e) =>
            {
                using var font = new Font("Segoe UI", 12F, FontStyle.Bold);
                var size = e.Graphics.MeasureString("HR", font);
                e.Graphics.DrawString("HR", font, Brushes.White,
                    (iconBox.Width - size.Width) / 2, (iconBox.Height - size.Height) / 2);
            };
            panelLogo.Controls.Add(iconBox);

            panelLogo.Controls.Add(new Label
            {
                Text = "HRM System", ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(64, 18), AutoSize = true
            });

            // panelLogo và divider sẽ được add sau, ở cuối sidebar setup

            // Cần đặt panelLogo trước divider trong dock order:
            // (WinForms: add sau → hiển thị trước cho DockStyle.Top)
            // panelMenu add order: panelNavItems (Fill) → divider (Top) → panelLogo (Top)
            // Khi render: panelLogo ở trên → divider → panelNavItems fill

            // --- Nav Items (scrollbar ẩn bằng clip panel) ---
            // Outer panel: clip overflow, không scroll
            panelNavItems = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Color.Transparent
            };

            // Inner panel: AutoScroll + rộng hơn 20px để đẩy scrollbar ra ngoài vùng nhìn thấy
            var navScrollPanel = new Panel
            {
                BackColor = Color.Transparent,
                AutoScroll = true,
                Location = new Point(0, 0),
                Padding = new Padding(12, 8, 12, 8)
            };
            // Set size khi outer panel resize
            panelNavItems.Resize += (s, e) =>
            {
                navScrollPanel.Size = new Size(panelNavItems.Width + 20, panelNavItems.Height);
            };
            panelNavItems.Controls.Add(navScrollPanel);
            _navInnerPanel = navScrollPanel;

            // --- User Area ---
            var panelUser = new Panel
            {
                Dock = DockStyle.Bottom, Height = 65,
                BackColor = Color.Transparent
            };
            panelUser.Paint += (s, e) =>
            {
                using var pen = new Pen(ThemeColors.SidebarDivider, 1);
                e.Graphics.DrawLine(pen, 0, 0, panelUser.Width, 0);
            };

            // Avatar circle
            var avatarCircle = new Panel
            {
                Size = new Size(36, 36), Location = new Point(24, 16),
                BackColor = ThemeColors.Primary
            };
            avatarCircle.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var initials = GetInitials(AppSession.DisplayName);
                using var font = new Font("Segoe UI", 11F, FontStyle.Bold);
                var size = e.Graphics.MeasureString(initials, font);
                e.Graphics.FillEllipse(new SolidBrush(ThemeColors.Primary), 0, 0, 35, 35);
                e.Graphics.DrawString(initials, font, Brushes.White,
                    (36 - size.Width) / 2, (36 - size.Height) / 2);
            };
            panelUser.Controls.Add(avatarCircle);

            panelUser.Controls.Add(new Label
            {
                Text = AppSession.DisplayName,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(68, 12), AutoSize = true
            });
            panelUser.Controls.Add(new Label
            {
                Text = AppSession.RoleDisplay,
                ForeColor = ThemeColors.SidebarText,
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(68, 32), AutoSize = true
            });

            // Nút đăng xuất nhỏ
            var btnLogout = new Label
            {
                Text = "⏻", ForeColor = ThemeColors.SidebarText,
                Font = new Font("Segoe UI", 14F),
                Location = new Point(220, 18), AutoSize = true,
                Cursor = Cursors.Hand
            };
            btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                { AppSession.Clear(); this.Close(); }
            };
            panelUser.Controls.Add(btnLogout);

            // --- Divider ---
            var divider = new Panel
            {
                Dock = DockStyle.Top, Height = 1,
                BackColor = ThemeColors.SidebarDivider
            };

            // WinForms dock order: add LAST = dock FIRST
            // Thứ tự add: Fill → Bottom → Top (divider) → Top (logo)
            // Kết quả: Logo trên cùng → divider → nav items fill → user dưới cùng
            panelMenu.Controls.Add(panelNavItems);  // Fill (add đầu)
            panelMenu.Controls.Add(panelUser);      // Bottom
            panelMenu.Controls.Add(divider);         // Top (dưới logo)
            panelMenu.Controls.Add(panelLogo);       // Top (trên cùng)

            // ===== CONTENT PANEL =====
            panelContent = new Panel
            {
                Dock = DockStyle.Fill, BackColor = ThemeColors.Background,
                Padding = new Padding(0)
            };
            panelContent.Controls.Add(new Label
            {
                Text = $"Chào mừng, {AppSession.DisplayName}!\n\nChọn một mục trong menu bên trái để bắt đầu.",
                ForeColor = ThemeColors.MutedForeground, Font = new Font("Segoe UI", 14F),
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter
            });

            // ADD TO FORM: Fill first, Left last
            this.Controls.Add(panelContent);
            this.Controls.Add(panelMenu);
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            return parts[0][..Math.Min(2, parts[0].Length)].ToUpper();
        }

        private void FrmMain_Load(object? sender, EventArgs e) => BuildMenuTree();

        private void BuildMenuTree()
        {
            _navInnerPanel.Controls.Clear();
            _menuFormMap.Clear();
            _navPanels.Clear();

            var tree = Program.MenuService.BuildPermissionTree(AppSession.Permissions);
            int yPos = 0;

            foreach (var root in tree)
            {
                if (!root.CanView && !root.Children.Any(c => c.CanView)) continue;

                // Nếu có children → tạo parent + children
                if (root.Children.Any(c => c.CanView))
                {
                    // Parent label (không click được)
                    foreach (var child in root.Children.Where(c => c.CanView))
                    {
                        var navItem = CreateNavItem(child, ref yPos);
                        if (navItem != null) _navInnerPanel.Controls.Add(navItem);
                    }
                }
                else if (!string.IsNullOrEmpty(root.FormName))
                {
                    var navItem = CreateNavItem(root, ref yPos);
                    if (navItem != null) _navInnerPanel.Controls.Add(navItem);
                }
            }
        }

        private Panel? CreateNavItem(MenuPermissionDto menu, ref int yPos)
        {
            if (!menu.CanView) return null;

            var icon = GetMenuIcon(menu.IconName);
            var panel = new Panel
            {
                Size = new Size(236, 42),
                Location = new Point(0, yPos),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Tag = menu.MenuCode
            };

            var lblIcon = new Label
            {
                Text = icon, ForeColor = ThemeColors.SidebarText,
                Font = new Font("Segoe UI", 12F),
                Location = new Point(16, 10), AutoSize = true
            };
            panel.Controls.Add(lblIcon);

            var lblText = new Label
            {
                Text = menu.MenuName, ForeColor = ThemeColors.SidebarText,
                Font = ThemeColors.FontSidebarItem,
                Location = new Point(48, 11), AutoSize = true
            };
            panel.Controls.Add(lblText);

            // Hover effect
            void SetHover(bool hover)
            {
                if (panel == _activeNavItem) return;
                panel.BackColor = hover ? ThemeColors.SidebarHover : Color.Transparent;
            }

            panel.MouseEnter += (s, e) => SetHover(true);
            panel.MouseLeave += (s, e) => SetHover(false);
            lblIcon.MouseEnter += (s, e) => SetHover(true);
            lblIcon.MouseLeave += (s, e) => SetHover(false);
            lblText.MouseEnter += (s, e) => SetHover(true);
            lblText.MouseLeave += (s, e) => SetHover(false);

            // Click
            void HandleClick(object? s, EventArgs e)
            {
                SetActiveNav(panel);
                if (!string.IsNullOrEmpty(menu.FormName))
                {
                    _menuFormMap[menu.MenuCode] = menu.FormName;
                    OpenChildForm(menu.FormName, menu.MenuCode);
                }
            }
            panel.Click += HandleClick;
            lblIcon.Click += HandleClick;
            lblText.Click += HandleClick;

            if (!string.IsNullOrEmpty(menu.FormName))
                _menuFormMap[menu.MenuCode] = menu.FormName;

            _navPanels[menu.MenuCode] = panel;
            yPos += 42;
            return panel;
        }

        private void SetActiveNav(Panel activePanel)
        {
            // Bỏ active cũ
            if (_activeNavItem != null)
            {
                _activeNavItem.BackColor = Color.Transparent;
                _activeNavItem.Paint -= ActiveNavPaint;
                _activeNavItem.Invalidate();
                foreach (Control c in _activeNavItem.Controls)
                    if (c is Label lbl) lbl.ForeColor = ThemeColors.SidebarText;
            }

            // Set active mới - nền sáng hơn + viền đỏ bên trái
            _activeNavItem = activePanel;
            activePanel.BackColor = ThemeColors.SidebarHover;
            activePanel.Paint += ActiveNavPaint;
            activePanel.Invalidate();
            foreach (Control c in activePanel.Controls)
                if (c is Label lbl) lbl.ForeColor = ThemeColors.SidebarActive;
        }

        private void ActiveNavPaint(object? sender, PaintEventArgs e)
        {
            // Vẽ viền đỏ bên trái 3px (accent bar)
            using var brush = new SolidBrush(ThemeColors.Primary);
            e.Graphics.FillRectangle(brush, 0, 4, 3, ((Control)sender!).Height - 8);
        }


        private string GetMenuIcon(string? iconName) => iconName switch
        {
            "people" => "👥", "user" => "👤", "organization" => "🏢", "badge" => "🎖️",
            "clock" => "⏰", "calendar-check" => "📅", "calendar" => "🗓️", "document" => "📄",
            "money" => "💰", "calculator" => "🧮", "receipt" => "🧾", "history" => "📜",
            "settings" => "⚙️", "chart" => "📊", "dashboard" => "📈", "report" => "📋",
            "shield" => "🛡️", "key" => "🔑", "menu" => "☰", "list" => "📝", "log" => "📒",
            _ => "▸"
        };

        private void OpenChildForm(string formName, string menuCode)
        {
            foreach (Control c in panelContent.Controls)
            {
                if (c is Form f) f.Close();
                c.Dispose();
            }
            panelContent.Controls.Clear();

            Form? childForm = formName switch
            {
                "FrmEmployeeList" => new FrmEmployeeList(),
                "FrmDepartmentList" => new FrmDepartmentList(),
                "FrmPositionList" => new FrmPositionList(),
                "FrmRolePermission" => new FrmRolePermission(),
                "FrmMenuManager" => new FrmMenuManager(),
                "FrmCategoryManager" => new FrmCategoryManager(),
                "FrmAttendanceDaily" => new FrmAttendanceDaily(),
                "FrmAttendanceMonthly" => new FrmAttendanceMonthly(),
                "FrmLeaveRequest" => new FrmLeaveRequest(),
                "FrmSalaryConfig" => new FrmSalaryConfig(),
                "FrmSalaryCalculation" => new FrmSalaryCalculation(),
                "FrmPayslip" or "FrmSalarySlip" => new FrmPayslip(),
                "FrmSalaryHistory" => new FrmSalaryHistory(),
                "FrmDashboard" => new FrmDashboard(),
                "FrmReportExport" or "FrmReportViewer" => new FrmReportExport(),
                "FrmRoleList" => new FrmRoleList(),
                "FrmAuditLog" => new FrmAuditLog(),
                "FrmUserManager" => new FrmUserManager(),
                "FrmChangePassword" => new FrmChangePassword(),
                "FrmBackupRestore" => new FrmBackupRestore(),
                "FrmCompanySettings" => new FrmCompanySettings(),
                "FrmContractManager" => new FrmContractManager(),
                "FrmEmployeeEvents" => new FrmEmployeeEvents(),
                "FrmAdvanceManager" => new FrmAdvanceManager(),
                _ => null
            };

            if (childForm != null)
            {
                FormHelper.EmbedForm(panelContent, childForm);
            }
            else
            {
                panelContent.Controls.Add(new Label
                {
                    Text = $"🚧 {formName}\n\nForm này sẽ được triển khai ở các phase tiếp theo.\n\nMenu Code: {menuCode}",
                    ForeColor = ThemeColors.MutedForeground, Font = new Font("Segoe UI", 14F),
                    Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter
                });
            }
        }
    }
}
