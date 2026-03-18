using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Salary
{
    /// <summary>
    /// Tính lương tháng: chọn tháng/năm → tính lương hàng loạt → duyệt
    /// </summary>
    public class FrmSalaryCalculation : Form
    {
        private DataGridView dgv = null!;
        private ComboBox cboMonth = null!, cboYear = null!, cboDept = null!;
        private Button btnCalc = null!, btnApprove = null!, btnRefresh = null!;
        private Label lblTotal = null!;
        private readonly string _menuCode = "LG_TINH";

        public FrmSalaryCalculation()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadFiltersAsync(); await LoadDataAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Tính Lương Tháng";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // Toolbar
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background,
                Padding = new Padding(8, 8, 0, 0)
            };
            btnCalc = CreateBtn("🧮 Tính Lương", "Add", ThemeColors.Primary, 140);
            btnApprove = CreateBtn("✅ Duyệt tất cả", "Edit", ThemeColors.Success, 150);
            btnRefresh = CreateBtn("🔄", "", ThemeColors.MutedForeground, 50);
            toolbar.Controls.AddRange(new Control[] { btnCalc, btnApprove, btnRefresh });

            btnCalc.Click += BtnCalc_Click;
            btnApprove.Click += BtnApprove_Click;
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            // Filter bar
            var filterBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background };

            filterBar.Controls.Add(new Label { Text = "📅 Tháng:", Location = new Point(10, 14), AutoSize = true, ForeColor = ThemeColors.Foreground });
            cboMonth = new ComboBox { Location = new Point(80, 10), Size = new Size(60, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            for (int i = 1; i <= 12; i++) cboMonth.Items.Add(i);
            cboMonth.SelectedItem = DateTime.Now.Month;
            filterBar.Controls.Add(cboMonth);

            filterBar.Controls.Add(new Label { Text = "Năm:", Location = new Point(150, 14), AutoSize = true, ForeColor = ThemeColors.Foreground });
            cboYear = new ComboBox { Location = new Point(195, 10), Size = new Size(80, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            for (int y = DateTime.Now.Year; y >= DateTime.Now.Year - 5; y--) cboYear.Items.Add(y);
            cboYear.SelectedIndex = 0;
            filterBar.Controls.Add(cboYear);

            filterBar.Controls.Add(new Label { Text = "Phòng ban:", Location = new Point(290, 14), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
            cboDept = new ComboBox { Location = new Point(375, 10), Size = new Size(180, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat };
            filterBar.Controls.Add(cboDept);

            cboMonth.SelectedIndexChanged += async (s, e) => await LoadDataAsync();
            cboYear.SelectedIndexChanged += async (s, e) => await LoadDataAsync();
            cboDept.SelectedIndexChanged += async (s, e) => await LoadDataAsync();

            lblTotal = new Label { Text = "", Location = new Point(570, 14), AutoSize = true, ForeColor = ThemeColors.Success, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            filterBar.Controls.Add(lblTotal);

            // Grid
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = ThemeColors.Background, GridColor = ThemeColors.Border,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground,
                    SelectionBackColor = ThemeColors.Primary, Font = new Font("Segoe UI", 10F)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = ThemeColors.Surface, ForeColor = ThemeColors.MutedForeground,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                },
                EnableHeadersVisualStyles = false, RowHeadersVisible = false, BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 40, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            dgv.CellFormatting += Dgv_CellFormatting;

            // WinForms dock order
            this.Controls.Add(dgv);
            this.Controls.Add(filterBar);
            this.Controls.Add(toolbar);
        }

        private Button CreateBtn(string text, string tag, Color c, int w = 120)
        {
            var b = new Button
            {
                Text = text, Tag = tag, Size = new Size(w, 35), FlatStyle = FlatStyle.Flat,
                BackColor = c, ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 8, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private async Task LoadFiltersAsync()
        {
            var depts = (await Program.DeptService.GetAllAsync()).ToList();
            depts.Insert(0, new Models.Entities.Department { DepartmentId = 0, DepartmentName = "-- Tất cả --" });
            cboDept.DataSource = depts;
            cboDept.DisplayMember = "DepartmentName";
            cboDept.ValueMember = "DepartmentId";
        }

        private async Task LoadDataAsync()
        {
            if (cboMonth.SelectedItem == null || cboYear.SelectedItem == null) return;
            var month = (int)cboMonth.SelectedItem;
            var year = (int)cboYear.SelectedItem;
            var deptId = cboDept.SelectedValue is int d && d > 0 ? d : (int?)null;

            var list = (await Program.SalaryService.GetRecordsAsync(month, year, deptId)).ToList();
            dgv.DataSource = null;
            dgv.DataSource = list;

            // Ẩn cột không cần
            var hideCols = new[] { "SalaryId", "EmployeeId", "Month", "Year", "SalaryCoefficient",
                "OtherAllowance", "SocialInsurance", "HealthInsurance", "UnemploymentInsurance",
                "PersonalDeduction", "DependentDeduction", "TaxableIncome", "OtherDeductions",
                "ApprovedBy", "Notes", "CreatedAt" };
            foreach (var col in hideCols)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].Visible = false;

            var headers = new Dictionary<string, string>
            {
                ["EmployeeCode"] = "Mã NV", ["EmployeeName"] = "Họ Tên", ["DepartmentName"] = "Phòng Ban",
                ["WorkingDays"] = "Ngày Công", ["StandardDays"] = "Chuẩn",
                ["BasicSalary"] = "Lương CB", ["PositionAllowance"] = "Phụ Cấp CV",
                ["OvertimePay"] = "Tăng Ca", ["GrossIncome"] = "Thu Nhập",
                ["PersonalIncomeTax"] = "Thuế TNCN", ["NetSalary"] = "Thực Lĩnh",
                ["Status"] = "Trạng Thái"
            };
            foreach (var (col, header) in headers)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].HeaderText = header;

            // Format money columns
            var moneyCols = new[] { "BasicSalary", "PositionAllowance", "OvertimePay", "GrossIncome", "PersonalIncomeTax", "NetSalary" };
            foreach (var col in moneyCols)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].DefaultCellStyle.Format = "N0";

            var totalNet = list.Sum(r => r.NetSalary);
            lblTotal.Text = $"💰 Tổng: {totalNet:N0} VNĐ | {list.Count} phiếu";
        }

        private void Dgv_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgv.Columns[e.ColumnIndex].Name == "Status" && e.Value is string status)
            {
                e.CellStyle!.ForeColor = status switch
                {
                    "Draft" => Color.FromArgb(255, 193, 7),
                    "Approved" => Color.FromArgb(76, 175, 80),
                    _ => ThemeColors.Foreground
                };
            }
        }

        private async void BtnCalc_Click(object? s, EventArgs e)
        {
            if (cboMonth.SelectedItem == null || cboYear.SelectedItem == null) return;
            var month = (int)cboMonth.SelectedItem;
            var year = (int)cboYear.SelectedItem;
            var deptId = cboDept.SelectedValue is int d && d > 0 ? d : (int?)null;

            if (MessageBox.Show($"Tính lương tháng {month}/{year}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            btnCalc.Enabled = false;
            btnCalc.Text = "⏳ Đang tính...";
            try
            {
                var (ok, msg, count) = await Program.SalaryService.CalculateMonthlyAsync(month, year, deptId);
                if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
                else FormHelper.ShowError(msg);
            }
            finally
            {
                btnCalc.Enabled = true;
                btnCalc.Text = "🧮 Tính Lương";
            }
        }

        private async void BtnApprove_Click(object? s, EventArgs e)
        {
            if (cboMonth.SelectedItem == null || cboYear.SelectedItem == null) return;
            var month = (int)cboMonth.SelectedItem;
            var year = (int)cboYear.SelectedItem;

            if (MessageBox.Show($"Duyệt tất cả phiếu lương tháng {month}/{year}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            var (ok, msg) = await Program.SalaryService.ApproveAllAsync(month, year, AppSession.CurrentUser?.UserId ?? 0);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
            else FormHelper.ShowError(msg);
        }
    }
}
