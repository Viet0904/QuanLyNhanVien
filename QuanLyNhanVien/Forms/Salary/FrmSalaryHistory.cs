using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Salary
{
    /// <summary>
    /// Lịch sử lương: xem các tháng lương của 1 nhân viên
    /// </summary>
    public class FrmSalaryHistory : Form
    {
        private DataGridView dgv = null!;
        private ComboBox cboEmployee = null!;
        private Label lblSummary = null!;
        private readonly string _menuCode = "LG_LICHSU";

        public FrmSalaryHistory()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadFiltersAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Lịch Sử Lương";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // Filter bar
            var filterBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background };

            filterBar.Controls.Add(new Label { Text = "👤 Nhân viên:", Location = new Point(10, 14), AutoSize = true, ForeColor = ThemeColors.Foreground });
            cboEmployee = new ComboBox
            {
                Location = new Point(110, 10), Size = new Size(280, 30), DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat
            };
            cboEmployee.SelectedIndexChanged += async (s, e) => await LoadDataAsync();
            filterBar.Controls.Add(cboEmployee);

            lblSummary = new Label
            {
                Text = "", Location = new Point(410, 14), AutoSize = true,
                ForeColor = ThemeColors.Success, Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            filterBar.Controls.Add(lblSummary);

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
        }

        private async Task LoadFiltersAsync()
        {
            var empResult = await Program.EmployeeService.GetListAsync(1, 9999, null, null, true);
            var employees = empResult.Items.ToList();
            employees.Insert(0, new Models.Entities.Employee { EmployeeId = 0, FullName = "-- Chọn nhân viên --" });
            cboEmployee.DataSource = employees;
            cboEmployee.DisplayMember = "FullName";
            cboEmployee.ValueMember = "EmployeeId";
        }

        private async Task LoadDataAsync()
        {
            if (cboEmployee.SelectedValue is not int empId || empId <= 0)
            {
                dgv.DataSource = null;
                lblSummary.Text = "";
                return;
            }

            var list = (await Program.SalaryService.GetHistoryAsync(empId)).ToList();
            dgv.DataSource = null;
            dgv.DataSource = list;

            // Ẩn cột
            var hideCols = new[] { "SalaryId", "EmployeeId", "EmployeeCode", "EmployeeName", "DepartmentName",
                "SalaryCoefficient", "OtherAllowance", "SocialInsurance", "HealthInsurance", "UnemploymentInsurance",
                "PersonalDeduction", "DependentDeduction", "TaxableIncome", "OtherDeductions",
                "ApprovedBy", "Notes", "CreatedAt" };
            foreach (var col in hideCols)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].Visible = false;

            var headers = new Dictionary<string, string>
            {
                ["Month"] = "Tháng", ["Year"] = "Năm", ["WorkingDays"] = "Ngày Công",
                ["StandardDays"] = "Chuẩn", ["BasicSalary"] = "Lương CB",
                ["PositionAllowance"] = "Phụ Cấp", ["OvertimePay"] = "Tăng Ca",
                ["GrossIncome"] = "Thu Nhập", ["PersonalIncomeTax"] = "Thuế TNCN",
                ["NetSalary"] = "Thực Lĩnh", ["Status"] = "Trạng Thái"
            };
            foreach (var (col, header) in headers)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].HeaderText = header;

            var moneyCols = new[] { "BasicSalary", "PositionAllowance", "OvertimePay", "GrossIncome", "PersonalIncomeTax", "NetSalary" };
            foreach (var col in moneyCols)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].DefaultCellStyle.Format = "N0";

            if (list.Any())
            {
                var avgNet = list.Average(r => r.NetSalary);
                lblSummary.Text = $"📊 {list.Count} tháng | TB thực lĩnh: {avgNet:N0} ₫";
            }
            else lblSummary.Text = "Chưa có dữ liệu lương.";
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
    }
}
