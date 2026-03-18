using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Employee
{
    public class FrmEmployeeDetail : Form
    {
        private TextBox txtCode = null!, txtName = null!, txtIdentity = null!, txtPhone = null!, txtEmail = null!, txtAddress = null!;
        private TextBox txtBankAccount = null!, txtBankName = null!, txtTaxCode = null!, txtInsurance = null!, txtNotes = null!;
        private ComboBox cboDept = null!, cboPosition = null!, cboGender = null!;
        private DateTimePicker dtpBirth = null!, dtpHire = null!;
        private NumericUpDown nudSalary = null!, nudCoeff = null!, nudDependents = null!;
        private Models.Entities.Employee? _emp;
        private TabControl tabs = null!;

        public FrmEmployeeDetail(Models.Entities.Employee? emp)
        {
            _emp = emp;
            InitializeComponent();
            this.Load += async (s, e) => await LoadCombosAsync();
        }

        private void InitializeComponent()
        {
            this.Text = _emp == null ? "Thêm nhân viên mới" : $"Sửa nhân viên - {_emp.FullName}";
            this.Size = new Size(650, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            tabs = new TabControl
            {
                Dock = DockStyle.Top, Height = 400,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(tabs);

            // == Tab 1: Thông tin chung ==
            var tabInfo = new TabPage("📋 Thông tin chung") { BackColor = ThemeColors.Background };
            tabs.TabPages.Add(tabInfo);

            var y = 15;
            AddLabel(tabInfo, "Mã nhân viên:", 20, y);
            txtCode = AddTextBox(tabInfo, 160, y, 180); txtCode.ReadOnly = true; txtCode.Text = "(Tự động)"; y += 38;
            AddLabel(tabInfo, "Họ và tên: *", 20, y);
            txtName = AddTextBox(tabInfo, 160, y, 250); y += 38;
            AddLabel(tabInfo, "Giới tính:", 20, y);
            cboGender = new ComboBox { Location = new Point(160, y), Size = new Size(120, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat };
            cboGender.Items.AddRange(new[] { "Nam", "Nữ", "Khác" });
            tabInfo.Controls.Add(cboGender); y += 38;
            AddLabel(tabInfo, "Ngày sinh:", 20, y);
            dtpBirth = new DateTimePicker { Location = new Point(160, y), Size = new Size(180, 30), Format = DateTimePickerFormat.Short };
            tabInfo.Controls.Add(dtpBirth); y += 38;
            AddLabel(tabInfo, "CMND/CCCD:", 20, y);
            txtIdentity = AddTextBox(tabInfo, 160, y, 200); y += 38;

            // == Tab 2: Liên hệ ==
            var tabContact = new TabPage("📞 Liên hệ") { BackColor = ThemeColors.Background };
            tabs.TabPages.Add(tabContact);
            y = 15;
            AddLabel(tabContact, "Số điện thoại:", 20, y);
            txtPhone = AddTextBox(tabContact, 160, y, 200); y += 38;
            AddLabel(tabContact, "Email:", 20, y);
            txtEmail = AddTextBox(tabContact, 160, y, 280); y += 38;
            AddLabel(tabContact, "Địa chỉ:", 20, y);
            txtAddress = AddTextBox(tabContact, 160, y, 400); y += 38;

            // == Tab 3: Công việc ==
            var tabWork = new TabPage("🏢 Công việc") { BackColor = ThemeColors.Background };
            tabs.TabPages.Add(tabWork);
            y = 15;
            AddLabel(tabWork, "Phòng ban: *", 20, y);
            cboDept = new ComboBox { Location = new Point(160, y), Size = new Size(250, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat };
            tabWork.Controls.Add(cboDept); y += 38;
            AddLabel(tabWork, "Chức vụ: *", 20, y);
            cboPosition = new ComboBox { Location = new Point(160, y), Size = new Size(250, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat };
            tabWork.Controls.Add(cboPosition); y += 38;
            AddLabel(tabWork, "Ngày vào làm:", 20, y);
            dtpHire = new DateTimePicker { Location = new Point(160, y), Size = new Size(180, 30), Format = DateTimePickerFormat.Short };
            tabWork.Controls.Add(dtpHire); y += 38;
            AddLabel(tabWork, "Ghi chú:", 20, y);
            txtNotes = AddTextBox(tabWork, 160, y, 400); txtNotes.Multiline = true; txtNotes.Height = 60;

            // == Tab 4: Tài chính ==
            var tabFinance = new TabPage("💰 Tài chính") { BackColor = ThemeColors.Background };
            tabs.TabPages.Add(tabFinance);
            y = 15;
            AddLabel(tabFinance, "Lương cơ bản:", 20, y);
            nudSalary = new NumericUpDown { Location = new Point(180, y), Size = new Size(200, 30), Maximum = 500000000, Increment = 500000, ThousandsSeparator = true, BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground };
            tabFinance.Controls.Add(nudSalary); y += 38;
            AddLabel(tabFinance, "Hệ số lương:", 20, y);
            nudCoeff = new NumericUpDown { Location = new Point(180, y), Size = new Size(100, 30), Minimum = 0.5M, Maximum = 20, DecimalPlaces = 2, Increment = 0.1M, Value = 1, BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground };
            tabFinance.Controls.Add(nudCoeff); y += 38;
            AddLabel(tabFinance, "Ngân hàng:", 20, y);
            txtBankName = AddTextBox(tabFinance, 180, y, 250); y += 38;
            AddLabel(tabFinance, "Số tài khoản:", 20, y);
            txtBankAccount = AddTextBox(tabFinance, 180, y, 250); y += 38;
            AddLabel(tabFinance, "Mã số thuế:", 20, y);
            txtTaxCode = AddTextBox(tabFinance, 180, y, 200); y += 38;
            AddLabel(tabFinance, "Số BHXH:", 20, y);
            txtInsurance = AddTextBox(tabFinance, 180, y, 200); y += 38;
            AddLabel(tabFinance, "Số người phụ thuộc:", 20, y);
            nudDependents = new NumericUpDown { Location = new Point(180, y), Size = new Size(100, 30), Minimum = 0, Maximum = 20, Value = 0, BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground };
            tabFinance.Controls.Add(nudDependents);

            // == Buttons ==
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 55, BackColor = ThemeColors.Surface,
                Padding = new Padding(180, 8, 0, 0), FlowDirection = FlowDirection.LeftToRight
            };
            var btnSave = ThemeColors.CreatePrimaryButton("💾 Lưu", 120, 40);
            btnSave.Click += BtnSave_Click;

            var btnSaveNew = new Button { Text = "💾 Lưu & Thêm mới", Size = new Size(160, 40), FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Success, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(8, 0, 0, 0) };
            btnSaveNew.FlatAppearance.BorderSize = 0;
            btnSaveNew.Click += BtnSaveNew_Click;

            var btnCancel = ThemeColors.CreateOutlineButton("Hủy", 80, 40);
            btnCancel.Margin = new Padding(8, 0, 0, 0);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            btnPanel.Controls.AddRange(new Control[] { btnSave, btnSaveNew, btnCancel });
            if (_emp != null) btnSaveNew.Visible = false;
            this.Controls.Add(btnPanel);

            // Load existing data
            if (_emp != null) FillForm();
        }

        private void FillForm()
        {
            txtCode.Text = _emp!.EmployeeCode;
            txtName.Text = _emp.FullName;
            cboGender.SelectedItem = _emp.Gender;
            if (_emp.DateOfBirth.HasValue) dtpBirth.Value = _emp.DateOfBirth.Value;
            txtIdentity.Text = _emp.IdentityNo;
            txtPhone.Text = _emp.Phone;
            txtEmail.Text = _emp.Email;
            txtAddress.Text = _emp.Address;
            dtpHire.Value = _emp.HireDate;
            txtNotes.Text = _emp.Notes;
            nudSalary.Value = _emp.BasicSalary;
            nudCoeff.Value = _emp.SalaryCoefficient;
            txtBankName.Text = _emp.BankName;
            txtBankAccount.Text = _emp.BankAccount;
            txtTaxCode.Text = _emp.TaxCode;
            txtInsurance.Text = _emp.InsuranceNo;
            nudDependents.Value = _emp.NumberOfDependents;
        }

        private async Task LoadCombosAsync()
        {
            var depts = (await Program.EmployeeService.GetDepartmentsAsync()).ToList();
            cboDept.DataSource = depts;
            cboDept.DisplayMember = "DepartmentName";
            cboDept.ValueMember = "DepartmentId";

            var positions = (await Program.EmployeeService.GetPositionsAsync()).ToList();
            cboPosition.DataSource = positions;
            cboPosition.DisplayMember = "PositionName";
            cboPosition.ValueMember = "PositionId";

            if (_emp != null)
            {
                cboDept.SelectedValue = _emp.DepartmentId;
                cboPosition.SelectedValue = _emp.PositionId;
            }
        }

        private Models.Entities.Employee BuildEmployee()
        {
            var emp = _emp ?? new Models.Entities.Employee();
            emp.FullName = txtName.Text.Trim();
            emp.Gender = cboGender.SelectedItem?.ToString();
            emp.DateOfBirth = dtpBirth.Value;
            emp.IdentityNo = txtIdentity.Text.Trim();
            emp.Phone = txtPhone.Text.Trim();
            emp.Email = txtEmail.Text.Trim();
            emp.Address = txtAddress.Text.Trim();
            emp.DepartmentId = (int)(cboDept.SelectedValue ?? 0);
            emp.PositionId = (int)(cboPosition.SelectedValue ?? 0);
            emp.HireDate = dtpHire.Value;
            emp.Notes = txtNotes.Text.Trim();
            emp.BasicSalary = nudSalary.Value;
            emp.SalaryCoefficient = nudCoeff.Value;
            emp.BankName = txtBankName.Text.Trim();
            emp.BankAccount = txtBankAccount.Text.Trim();
            emp.TaxCode = txtTaxCode.Text.Trim();
            emp.InsuranceNo = txtInsurance.Text.Trim();
            emp.NumberOfDependents = (int)nudDependents.Value;
            return emp;
        }

        private async void BtnSave_Click(object? s, EventArgs e) => await SaveAsync(false);
        private async void BtnSaveNew_Click(object? s, EventArgs e) => await SaveAsync(true);

        private async Task SaveAsync(bool addNew)
        {
            var emp = BuildEmployee();
            if (_emp == null)
            {
                var (ok, msg, id) = await Program.EmployeeService.CreateAsync(emp);
                if (!ok) { FormHelper.ShowError(msg); return; }
                FormHelper.ShowSuccess(msg);
                if (addNew) ClearForm();
                else { this.DialogResult = DialogResult.OK; this.Close(); }
            }
            else
            {
                var (ok, msg) = await Program.EmployeeService.UpdateAsync(emp);
                if (!ok) { FormHelper.ShowError(msg); return; }
                FormHelper.ShowSuccess(msg);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void ClearForm()
        {
            _emp = null;
            txtCode.Text = "(Tự động)";
            txtName.Clear(); txtIdentity.Clear(); txtPhone.Clear(); txtEmail.Clear(); txtAddress.Clear();
            txtBankAccount.Clear(); txtBankName.Clear(); txtTaxCode.Clear(); txtInsurance.Clear(); txtNotes.Clear();
            nudSalary.Value = 0; nudCoeff.Value = 1; nudDependents.Value = 0;
            cboGender.SelectedIndex = -1;
            tabs.SelectedIndex = 0;
            txtName.Focus();
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label { Text = text, Location = new Point(x, y + 3), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
        }

        private TextBox AddTextBox(Control parent, int x, int y, int w)
        {
            var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 30), BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, BorderStyle = BorderStyle.FixedSingle };
            parent.Controls.Add(tb);
            return tb;
        }
    }
}
