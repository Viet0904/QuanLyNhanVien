using QuanLyNhanVien.Helpers;

namespace QuanLyNhanVien.Forms.Report
{
    /// <summary>
    /// Dashboard tổng quan: KPI cards + bảng nhân viên + cảnh báo
    /// Theme sáng theo thiết kế Pencil
    /// </summary>
    public class FrmDashboard : Form
    {
        private FlowLayoutPanel panelCards = null!;
        private Panel panelBottom = null!;
        private DataGridView dgvRecent = null!;
        private Panel panelSideStats = null!;
        private readonly string _menuCode = "BC_DASHBOARD";

        // Data
        private List<(string DeptName, int Count)> _deptData = new();
        private List<(int Month, decimal TotalNet, int RecordCount)> _salaryData = new();
        private List<dynamic> _topSalary = new();

        public FrmDashboard()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadDashboardAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Tổng quan";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;
            this.DoubleBuffered = true;
            this.Padding = new Padding(32, 24, 32, 24);

            // ===== PAGE HEADER =====
            var btnExport = ThemeColors.CreateOutlineButton("📥 Xuất dữ liệu", 140);
            btnExport.Tag = "Export";
            var btnAdd = ThemeColors.CreatePrimaryButton("+ Thêm mới", 120);
            btnAdd.Tag = "Add";

            var header = ThemeColors.CreatePageHeader(
                "Tổng quan",
                "Chào mừng trở lại, Admin. Đây là tổng quan nhân sự hôm nay.",
                new Control[] { btnExport, btnAdd }
            );
            this.Controls.Add(header);

            // ===== KPI CARDS =====
            panelCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 120,
                BackColor = ThemeColors.Background,
                Padding = new Padding(0, 8, 0, 8),
                WrapContents = false, AutoScroll = false
            };
            this.Controls.Add(panelCards);

            // ===== BOTTOM SECTION: Table + Side Stats =====
            panelBottom = new Panel
            {
                Dock = DockStyle.Fill, BackColor = ThemeColors.Background,
                Padding = new Padding(0, 16, 0, 0)
            };

            // Side stats panel (bên phải)
            panelSideStats = new Panel
            {
                Dock = DockStyle.Right, Width = 340,
                BackColor = ThemeColors.Background,
                Padding = new Padding(24, 0, 0, 0)
            };
            panelBottom.Controls.Add(panelSideStats);

            // Employee table (bên trái, fill)
            var panelTable = new Panel
            {
                Dock = DockStyle.Fill, BackColor = ThemeColors.Card
            };
            panelTable.Paint += (s, e) =>
            {
                using var pen = new Pen(ThemeColors.Border, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panelTable.Width - 1, panelTable.Height - 1);
            };

            // Table header
            var tableHeader = new Panel
            {
                Dock = DockStyle.Top, Height = 50,
                BackColor = ThemeColors.Card,
                Padding = new Padding(20, 12, 20, 12)
            };
            tableHeader.Paint += (s, e) =>
            {
                using var pen = new Pen(ThemeColors.Border, 1);
                e.Graphics.DrawLine(pen, 0, tableHeader.Height - 1, tableHeader.Width, tableHeader.Height - 1);
            };
            tableHeader.Controls.Add(new Label
            {
                Text = "Nhân viên mới gần đây", ForeColor = ThemeColors.Foreground,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(20, 12), AutoSize = true
            });
            panelTable.Controls.Add(tableHeader);

            // DataGridView
            dgvRecent = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            ThemeColors.StyleDataGridView(dgvRecent);

            // Add table content → fill first, header top
            panelTable.Controls.Add(dgvRecent);

            panelBottom.Controls.Add(panelTable);
            this.Controls.Add(panelBottom);
        }

        private async Task LoadDashboardAsync()
        {
            var stats = await Program.ReportService.GetDashboardStatsAsync();
            _deptData = (await Program.ReportService.GetDepartmentDistributionAsync()).ToList();
            _salaryData = (await Program.ReportService.GetMonthlySalarySummaryAsync(DateTime.Now.Year)).ToList();
            _topSalary = (await Program.ReportService.GetTopSalaryAsync(DateTime.Now.Month, DateTime.Now.Year, 5)).ToList();

            var thisMonthSalary = _salaryData.FirstOrDefault(s => s.Month == DateTime.Now.Month);
            var totalSalary = thisMonthSalary.TotalNet;

            BuildKPICards(stats.TotalEmployees, stats.TotalDepts, stats.NewThisMonth, totalSalary);
            BuildSideStats();
            await LoadRecentEmployees();
        }

        private void BuildKPICards(int totalEmp, int totalDept, int newMonth, decimal totalSalary)
        {
            panelCards.Controls.Clear();

            var cards = new[]
            {
                ("Tổng nhân viên", totalEmp.ToString("N0"), $"+5.2%", ThemeColors.Success),
                ("Đi làm hôm nay", (totalEmp - newMonth).ToString("N0"), "95%", ThemeColors.Success),
                ("Nghỉ phép", newMonth.ToString(), "-8%", ThemeColors.Warning),
                ("Quỹ lương tháng", FormatCurrency(totalSalary), "+3.1%", ThemeColors.Success)
            };

            int cardWidth = (panelCards.ClientSize.Width - 80) / 4;
            foreach (var (label, value, badge, badgeColor) in cards)
            {
                var card = ThemeColors.CreateMetricCard(label, value, badge, badgeColor);
                card.Size = new Size(cardWidth, 100);
                card.Margin = new Padding(0, 0, 20, 0);
                panelCards.Controls.Add(card);
            }
        }

        private static string FormatCurrency(decimal amount)
        {
            if (amount >= 1_000_000_000) return $"{amount / 1_000_000_000:N1} tỷ";
            if (amount >= 1_000_000) return $"{amount / 1_000_000:N0} triệu";
            return $"{amount:N0} ₫";
        }

        private async Task LoadRecentEmployees()
        {
            try
            {
                var result = await Program.EmployeeService.GetListAsync(1, 5, "", null, true);
                dgvRecent.DataSource = result.Items.ToList();

                var hideCols = new[] { "EmployeeId", "UserId", "Photo", "DepartmentId", "PositionId",
                    "CreatedAt", "UpdatedAt", "BankAccount", "BankName", "TaxCode", "InsuranceNo",
                    "SalaryCoefficient", "BasicSalary", "Notes", "TerminationDate", "Address",
                    "IdentityNo", "Gender", "Email", "DateOfBirth" };
                foreach (var col in hideCols)
                    if (dgvRecent.Columns.Contains(col)) dgvRecent.Columns[col].Visible = false;

                var headers = new Dictionary<string, string>
                {
                    ["EmployeeCode"] = "Mã NV", ["FullName"] = "Họ tên",
                    ["DepartmentName"] = "Phòng ban", ["PositionName"] = "Chức vụ",
                    ["Phone"] = "Điện thoại", ["HireDate"] = "Ngày vào làm",
                    ["IsActive"] = "Trạng thái"
                };
                foreach (var (col, h) in headers)
                    if (dgvRecent.Columns.Contains(col)) dgvRecent.Columns[col].HeaderText = h;
            }
            catch { /* Bảng sẽ trống nếu lỗi */ }
        }

        private void BuildSideStats()
        {
            panelSideStats.Controls.Clear();

            // --- Sinh nhật tháng này ---
            var birthdayPanel = CreateSideCard("🎂 Sinh nhật tháng này", 200);
            birthdayPanel.Dock = DockStyle.Top;
            birthdayPanel.Controls.Add(new Label
            {
                Text = "Nguyễn Văn Nam\nTrần Thị Lan\nLê Hoàng Minh",
                ForeColor = ThemeColors.Foreground,
                Font = ThemeColors.FontBody,
                Location = new Point(20, 50), AutoSize = true
            });
            panelSideStats.Controls.Add(birthdayPanel);

            // --- Hợp đồng sắp hết hạn ---
            var contractPanel = CreateSideCard("⚠️ Hợp đồng sắp hết hạn", 200);
            contractPanel.Dock = DockStyle.Top;

            var alerts = new[]
            {
                ("Phạm Minh Tuấn", "Còn 15 ngày", ThemeColors.Warning),
                ("Lê Thị Hương", "Còn 7 ngày", ThemeColors.Error)
            };
            int ay = 50;
            foreach (var (name, detail, color) in alerts)
            {
                contractPanel.Controls.Add(new Label
                {
                    Text = name, ForeColor = ThemeColors.Foreground,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    Location = new Point(20, ay), AutoSize = true
                });
                contractPanel.Controls.Add(new Label
                {
                    Text = detail, ForeColor = color,
                    Font = new Font("Segoe UI", 9F),
                    Location = new Point(20, ay + 18), AutoSize = true
                });
                ay += 45;
            }
            panelSideStats.Controls.Add(contractPanel);
        }

        private Panel CreateSideCard(string title, int height)
        {
            var panel = new Panel
            {
                Size = new Size(316, height), BackColor = ThemeColors.Card,
                Margin = new Padding(0, 0, 0, 16)
            };
            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(ThemeColors.Border, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };
            panel.Controls.Add(new Label
            {
                Text = title, ForeColor = ThemeColors.Foreground,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(20, 16), AutoSize = true
            });
            return panel;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (panelCards != null && panelCards.IsHandleCreated)
            {
                _ = LoadDashboardAsync();
            }
        }
    }
}
