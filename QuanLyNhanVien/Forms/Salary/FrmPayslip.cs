using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Salary
{
    /// <summary>
    /// Phiếu lương chi tiết cho 1 nhân viên
    /// </summary>
    public class FrmPayslip : Form
    {
        private ComboBox cboMonth = null!, cboYear = null!, cboEmployee = null!;
        private Panel panelSlip = null!;
        private readonly string _menuCode = "LG_PHIEU";

        public FrmPayslip()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadFiltersAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Phiếu Lương";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

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

            filterBar.Controls.Add(new Label { Text = "Nhân viên:", Location = new Point(290, 14), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
            cboEmployee = new ComboBox { Location = new Point(375, 10), Size = new Size(250, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat };
            filterBar.Controls.Add(cboEmployee);

            var btnView = new Button
            {
                Text = "📋 Xem", Size = new Size(100, 35), Location = new Point(640, 8),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Primary,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnView.FlatAppearance.BorderSize = 0;
            btnView.Click += async (s, e) => await LoadPayslipAsync();
            filterBar.Controls.Add(btnView);

            // Payslip content
            panelSlip = new Panel
            {
                Dock = DockStyle.Fill, BackColor = ThemeColors.Background,
                AutoScroll = true, Padding = new Padding(30)
            };

            // WinForms dock order
            this.Controls.Add(panelSlip);
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

        private async Task LoadPayslipAsync()
        {
            panelSlip.Controls.Clear();

            if (cboEmployee.SelectedValue is not int empId || empId <= 0)
            {
                AddSlipLabel("Vui lòng chọn nhân viên.", 30, 30, 14, Color.FromArgb(255, 193, 7));
                return;
            }

            var month = (int)cboMonth.SelectedItem!;
            var year = (int)cboYear.SelectedItem!;

            var records = await Program.SalaryService.GetRecordsAsync(month, year);
            var record = records.FirstOrDefault(r => r.EmployeeId == empId);

            if (record == null)
            {
                AddSlipLabel("Chưa có phiếu lương cho nhân viên này trong tháng đã chọn.", 30, 30, 12, Color.FromArgb(255, 152, 0));
                return;
            }

            RenderPayslip(record, month, year);
        }

        private void RenderPayslip(SalaryRecord r, int month, int year)
        {
            var y = 10;
            var headerColor = ThemeColors.Primary;
            var titleColor = ThemeColors.Foreground;
            var valueColor = Color.FromArgb(230, 240, 255);
            var subtitleColor = Color.FromArgb(140, 150, 180);

            // Header
            AddSlipLabel("PHIẾU LƯƠNG", 30, y, 16, headerColor, FontStyle.Bold); y += 35;
            AddSlipLabel($"Tháng {month}/{year}", 30, y, 12, subtitleColor); y += 35;

            // Thông tin NV
            AddSection("👤 THÔNG TIN NHÂN VIÊN", ref y);
            AddRow("Mã NV:", r.EmployeeCode ?? "", ref y);
            AddRow("Họ tên:", r.EmployeeName ?? "", ref y);
            AddRow("Phòng ban:", r.DepartmentName ?? "", ref y);
            AddRow("Trạng thái:", r.Status, ref y);
            y += 10;

            // Thu nhập
            AddSection("💰 THU NHẬP", ref y);
            AddRow("Lương cơ bản:", FormatMoney(r.BasicSalary), ref y);
            AddRow("Hệ số lương:", r.SalaryCoefficient.ToString("N2"), ref y);
            AddRow("Ngày công:", $"{r.WorkingDays:N1} / {r.StandardDays:N1}", ref y);
            AddRow("Phụ cấp chức vụ:", FormatMoney(r.PositionAllowance), ref y);
            AddRow("Tăng ca:", FormatMoney(r.OvertimePay), ref y);
            AddRow("TỔNG THU NHẬP:", FormatMoney(r.GrossIncome), ref y, Color.FromArgb(76, 175, 80), true);
            y += 10;

            // Các khoản trừ
            AddSection("📉 CÁC KHOẢN TRỪ", ref y);
            AddRow("BHXH (8%):", FormatMoney(r.SocialInsurance), ref y);
            AddRow("BHYT (1.5%):", FormatMoney(r.HealthInsurance), ref y);
            AddRow("BHTN (1%):", FormatMoney(r.UnemploymentInsurance), ref y);
            AddRow("Giảm trừ bản thân:", FormatMoney(r.PersonalDeduction), ref y);
            AddRow("Giảm trừ NPT:", FormatMoney(r.DependentDeduction), ref y);
            AddRow("Thu nhập chịu thuế:", FormatMoney(r.TaxableIncome), ref y);
            AddRow("Thuế TNCN:", FormatMoney(r.PersonalIncomeTax), ref y, Color.FromArgb(244, 67, 54));
            y += 10;

            // Thực lĩnh
            AddSection("✅ THỰC LĨNH", ref y);
            AddRow("LƯƠNG THỰC LĨNH:", FormatMoney(r.NetSalary), ref y, Color.FromArgb(76, 175, 80), true, 14);
        }

        private void AddSection(string title, ref int y)
        {
            var lbl = new Label
            {
                Text = title, Location = new Point(30, y), AutoSize = true,
                ForeColor = ThemeColors.Primary, Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            panelSlip.Controls.Add(lbl);
            y += 30;

            // Divider line
            var line = new Panel { Location = new Point(30, y), Size = new Size(500, 1), BackColor = Color.FromArgb(60, 60, 90) };
            panelSlip.Controls.Add(line);
            y += 8;
        }

        private void AddRow(string label, string value, ref int y, Color? valueColor = null, bool bold = false, int fontSize = 10)
        {
            panelSlip.Controls.Add(new Label
            {
                Text = label, Location = new Point(50, y), Size = new Size(200, 25),
                ForeColor = Color.FromArgb(160, 170, 200),
                Font = new Font("Segoe UI", fontSize, bold ? FontStyle.Bold : FontStyle.Regular)
            });
            panelSlip.Controls.Add(new Label
            {
                Text = value, Location = new Point(260, y), Size = new Size(250, 25),
                ForeColor = valueColor ?? Color.FromArgb(220, 230, 250),
                Font = new Font("Segoe UI", fontSize, bold ? FontStyle.Bold : FontStyle.Regular),
                TextAlign = ContentAlignment.TopRight
            });
            y += 28;
        }

        private void AddSlipLabel(string text, int x, int y, int size, Color color, FontStyle style = FontStyle.Regular)
        {
            panelSlip.Controls.Add(new Label
            {
                Text = text, Location = new Point(x, y), AutoSize = true,
                ForeColor = color, Font = new Font("Segoe UI", size, style)
            });
        }

        private static string FormatMoney(decimal value) => $"{value:N0} ₫";
    }
}
