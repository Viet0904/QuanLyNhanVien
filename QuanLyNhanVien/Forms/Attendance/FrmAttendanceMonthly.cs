using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Attendance
{
    public class FrmAttendanceMonthly : Form
    {
        private DataGridView dgv = null!;
        private ComboBox cboMonth = null!, cboYear = null!, cboDept = null!;
        private Button btnRefresh = null!;
        private Label lblSummary = null!;
        private readonly string _menuCode = "CC_THANG";

        public FrmAttendanceMonthly()
        {
            InitializeComponent();
            this.Load += async (s, e) =>
            {
                FormHelper.ApplyPermissions(this, _menuCode);
                await LoadFiltersAsync();
                await LoadDataAsync();
            };
        }

        private void InitializeComponent()
        {
            this.Text = "Bảng Chấm Công Tháng";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // === Filter bar ===
            var filterBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background };


            filterBar.Controls.Add(new Label { Text = "📅 Tháng:", Location = new Point(10, 14), AutoSize = true, ForeColor = ThemeColors.Foreground });
            cboMonth = new ComboBox { Location = new Point(80, 10), Size = new Size(60, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            for (int m = 1; m <= 12; m++) cboMonth.Items.Add(m);
            cboMonth.SelectedItem = DateTime.Now.Month;
            filterBar.Controls.Add(cboMonth);

            filterBar.Controls.Add(new Label { Text = "Năm:", Location = new Point(150, 14), AutoSize = true, ForeColor = ThemeColors.Foreground });
            cboYear = new ComboBox { Location = new Point(195, 10), Size = new Size(80, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            for (int y = DateTime.Now.Year - 2; y <= DateTime.Now.Year + 1; y++) cboYear.Items.Add(y);
            cboYear.SelectedItem = DateTime.Now.Year;
            filterBar.Controls.Add(cboYear);

            filterBar.Controls.Add(new Label { Text = "Phòng ban:", Location = new Point(290, 14), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
            cboDept = new ComboBox { Location = new Point(370, 10), Size = new Size(180, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat };
            filterBar.Controls.Add(cboDept);

            btnRefresh = new Button { Text = "🔄 Xem", Location = new Point(570, 8), Size = new Size(90, 35), BackColor = ThemeColors.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += async (s, e) => await LoadDataAsync();
            filterBar.Controls.Add(btnRefresh);

            lblSummary = new Label { Text = "", Location = new Point(680, 14), AutoSize = true, ForeColor = ThemeColors.Success, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            filterBar.Controls.Add(lblSummary);

            // === DataGridView ===
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                BackgroundColor = ThemeColors.Background, GridColor = ThemeColors.Border,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground,
                    SelectionBackColor = Color.FromArgb(60, 60, 90), Font = new Font("Segoe UI", 9F),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = ThemeColors.Surface, ForeColor = ThemeColors.MutedForeground,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                EnableHeadersVisualStyles = false, RowHeadersVisible = false, BorderStyle = BorderStyle.None
            };
            dgv.CellFormatting += Dgv_CellFormatting;
            // WinForms dock order: Fill first, Top after
            this.Controls.Add(dgv);         // Fill
            this.Controls.Add(filterBar);   // Top
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
            int month = (int)(cboMonth.SelectedItem ?? DateTime.Now.Month);
            int year = (int)(cboYear.SelectedItem ?? DateTime.Now.Year);
            var deptId = cboDept.SelectedValue is int d && d > 0 ? d : (int?)null;

            var records = (await Program.AttendanceService.GetMonthlyAsync(month, year, deptId)).ToList();
            int daysInMonth = DateTime.DaysInMonth(year, month);

            BuildMonthlyGrid(records, month, year, daysInMonth);
        }

        private void BuildMonthlyGrid(List<AttendanceRecord> records, int month, int year, int daysInMonth)
        {
            dgv.DataSource = null;
            dgv.Columns.Clear();
            dgv.Rows.Clear();

            // Cột cố định: Mã NV, Họ Tên
            dgv.Columns.Add("EmployeeCode", "Mã NV");
            dgv.Columns["EmployeeCode"]!.Width = 80;
            dgv.Columns["EmployeeCode"]!.Frozen = true;

            dgv.Columns.Add("EmployeeName", "Họ Tên");
            dgv.Columns["EmployeeName"]!.Width = 140;
            dgv.Columns["EmployeeName"]!.Frozen = true;

            // Cột cho từng ngày trong tháng
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var colName = $"D{day}";
                dgv.Columns.Add(colName, day.ToString());
                dgv.Columns[colName]!.Width = 32;

                // Đánh dấu weekend bằng header color
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    dgv.Columns[colName]!.HeaderCell.Style.BackColor = Color.FromArgb(70, 50, 50);
                    dgv.Columns[colName]!.HeaderCell.Style.ForeColor = Color.FromArgb(255, 150, 150);
                }
            }

            // Cột tổng
            dgv.Columns.Add("TotalDays", "Công");
            dgv.Columns["TotalDays"]!.Width = 50;
            dgv.Columns.Add("TotalOT", "TC");
            dgv.Columns["TotalOT"]!.Width = 45;

            // Group records by employee
            var grouped = records.GroupBy(r => new { r.EmployeeId, r.EmployeeCode, r.EmployeeName })
                                 .OrderBy(g => g.Key.EmployeeCode);

            int totalEmployees = 0;
            foreach (var group in grouped)
            {
                totalEmployees++;
                var row = new object[daysInMonth + 4]; // code + name + days + total + OT
                row[0] = group.Key.EmployeeCode ?? "";
                row[1] = group.Key.EmployeeName ?? "";

                decimal workDays = 0, totalOT = 0;

                foreach (var rec in group)
                {
                    int dayIndex = rec.WorkDate.Day - 1 + 2; // offset by 2 for code + name columns
                    row[dayIndex] = GetStatusSymbol(rec.Status);

                    if (rec.Status == "Present" || rec.Status == "Late" || rec.Status == "EarlyLeave")
                        workDays++;
                    if (rec.Status == "OnLeave")
                        workDays += 0; // không tính công

                    totalOT += rec.OvertimeHours;
                }

                row[daysInMonth + 2] = workDays;
                row[daysInMonth + 3] = totalOT > 0 ? totalOT : (object)"";

                dgv.Rows.Add(row);
            }

            lblSummary.Text = $"📊 {totalEmployees} nhân viên | Tháng {month}/{year}";
        }

        private string GetStatusSymbol(string status)
        {
            return status switch
            {
                "Present" => "✓",
                "Late" => "T",
                "EarlyLeave" => "S",
                "Absent" => "X",
                "OnLeave" => "P",
                "Holiday" => "L",
                _ => "-"
            };
        }

        private void Dgv_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 2 || e.Value == null) return;

            var val = e.Value.ToString();
            e.CellStyle!.ForeColor = val switch
            {
                "✓" => ThemeColors.Success,
                "T" => Color.FromArgb(255, 200, 60),
                "S" => Color.FromArgb(255, 160, 50),
                "X" => Color.FromArgb(220, 70, 70),
                "P" => Color.FromArgb(100, 170, 255),
                "L" => Color.FromArgb(180, 130, 255),
                _ => ThemeColors.Foreground
            };
            e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }
}
