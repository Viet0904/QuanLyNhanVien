using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Attendance
{
    public class FrmAttendanceDaily : Form
    {
        private DataGridView dgv = null!;
        private Button btnAdd = null!, btnEdit = null!, btnDelete = null!, btnBulk = null!, btnRefresh = null!;
        private DateTimePicker dtpDate = null!;
        private ComboBox cboDept = null!;
        private Label lblCount = null!;
        private readonly string _menuCode = "CC_NGAY";

        public FrmAttendanceDaily()
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
            this.Text = "Chấm Công Ngày";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // === Toolbar ===
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background,
                Padding = new Padding(8, 8, 0, 0)
            };
            btnAdd = CreateBtn("➕ Thêm", "Add", ThemeColors.Primary);
            btnEdit = CreateBtn("✏️ Sửa", "Edit", ThemeColors.Success);
            btnDelete = CreateBtn("🗑️ Xóa", "Delete", ThemeColors.Error);
            btnBulk = CreateBtn("📋 Hàng loạt", "Add", Color.FromArgb(156, 120, 243));
            btnRefresh = CreateBtn("🔄", "", ThemeColors.MutedForeground);
            btnRefresh.Size = new Size(50, 35);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnBulk, btnRefresh });


            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnBulk.Click += BtnBulk_Click;
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            // === Filter bar ===
            var filterBar = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = ThemeColors.Background };


            filterBar.Controls.Add(new Label { Text = "📅 Ngày:", Location = new Point(10, 12), AutoSize = true, ForeColor = ThemeColors.Foreground });
            dtpDate = new DateTimePicker
            {
                Location = new Point(80, 8), Size = new Size(150, 30), Format = DateTimePickerFormat.Short,
                Value = DateTime.Today, CalendarMonthBackground = ThemeColors.Surface,
                CalendarForeColor = ThemeColors.Foreground
            };
            dtpDate.ValueChanged += async (s, e) => await LoadDataAsync();
            filterBar.Controls.Add(dtpDate);

            filterBar.Controls.Add(new Label { Text = "Phòng ban:", Location = new Point(250, 12), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
            cboDept = new ComboBox
            {
                Location = new Point(330, 8), Size = new Size(180, 30), DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat
            };
            cboDept.SelectedIndexChanged += async (s, e) => await LoadDataAsync();
            filterBar.Controls.Add(cboDept);

            lblCount = new Label
            {
                Text = "", Location = new Point(530, 12), AutoSize = true,
                ForeColor = ThemeColors.Success, Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            filterBar.Controls.Add(lblCount);

            // === DataGridView ===
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
                EnableHeadersVisualStyles = false, RowHeadersVisible = false, BorderStyle = BorderStyle.None
            };
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };
            dgv.CellFormatting += Dgv_CellFormatting;
            // WinForms dock order: Fill first, then Top panels
            this.Controls.Add(dgv);         // Fill
            this.Controls.Add(filterBar);   // Top
            this.Controls.Add(toolbar);     // Top
        }

        private Button CreateBtn(string text, string tag, Color c)
        {
            var b = new Button
            {
                Text = text, Tag = tag, Size = new Size(130, 35), FlatStyle = FlatStyle.Flat,
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
            var deptId = cboDept.SelectedValue is int d && d > 0 ? d : (int?)null;
            var list = (await Program.AttendanceService.GetDailyAsync(dtpDate.Value.Date, deptId)).ToList();

            dgv.DataSource = null;
            dgv.DataSource = list;

            // Ẩn cột
            var hideCols = new[] { "RecordId", "EmployeeId", "CreatedBy", "CreatedAt" };
            foreach (var col in hideCols)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].Visible = false;

            // Đặt tên cột
            var headers = new Dictionary<string, string>
            {
                ["EmployeeCode"] = "Mã NV", ["EmployeeName"] = "Họ Tên", ["DepartmentName"] = "Phòng Ban",
                ["WorkDate"] = "Ngày", ["ShiftType"] = "Ca", ["CheckIn"] = "Giờ Vào", ["CheckOut"] = "Giờ Ra",
                ["Status"] = "Trạng Thái", ["OvertimeHours"] = "Tăng Ca (h)", ["Notes"] = "Ghi Chú"
            };
            foreach (var (col, header) in headers)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].HeaderText = header;

            lblCount.Text = $"📊 {list.Count} bản ghi";
        }

        /// <summary>
        /// Tô màu cell trạng thái
        /// </summary>
        private void Dgv_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgv.Columns[e.ColumnIndex].Name != "Status" || e.Value == null) return;

            var status = e.Value.ToString();
            e.CellStyle!.ForeColor = status switch
            {
                "Present" => ThemeColors.Success,
                "Late" => Color.FromArgb(255, 200, 60),
                "Absent" => Color.FromArgb(220, 70, 70),
                "OnLeave" => Color.FromArgb(100, 170, 255),
                "Holiday" => Color.FromArgb(180, 130, 255),
                _ => Color.White
            };
            e.CellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }

        private AttendanceRecord? GetSelected() => dgv.CurrentRow?.DataBoundItem as AttendanceRecord;

        // ==================== CRUD ====================

        private async void BtnAdd_Click(object? s, EventArgs e)
        {
            var dlg = new DlgAttendanceEntry(dtpDate.Value.Date, null);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnEdit_Click(object? s, EventArgs e)
        {
            var record = GetSelected();
            if (record == null) { FormHelper.ShowError("Vui lòng chọn bản ghi."); return; }
            var dlg = new DlgAttendanceEntry(dtpDate.Value.Date, record);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnDelete_Click(object? s, EventArgs e)
        {
            var record = GetSelected();
            if (record == null) { FormHelper.ShowError("Vui lòng chọn bản ghi."); return; }
            if (!FormHelper.ConfirmDelete($"chấm công của {record.EmployeeName}")) return;
            var (ok, msg) = await Program.AttendanceService.DeleteAsync(record.RecordId);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
            else FormHelper.ShowError(msg);
        }

        private async void BtnBulk_Click(object? s, EventArgs e)
        {
            var dlg = new DlgBulkAttendance(dtpDate.Value.Date);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }
    }

    // ==================== Dialog: Thêm/Sửa chấm công ====================
    public class DlgAttendanceEntry : Form
    {
        private ComboBox cboEmployee = null!, cboShift = null!, cboStatus = null!;
        private DateTimePicker dtpCheckIn = null!, dtpCheckOut = null!;
        private CheckBox chkCheckIn = null!, chkCheckOut = null!;
        private TextBox txtNotes = null!;
        private readonly DateTime _workDate;
        private readonly AttendanceRecord? _existing;

        public DlgAttendanceEntry(DateTime workDate, AttendanceRecord? existing)
        {
            _workDate = workDate;
            _existing = existing;
            InitializeComponent();
            this.Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeComponent()
        {
            this.Text = _existing == null ? "Thêm Chấm Công" : "Sửa Chấm Công";
            this.Size = new Size(450, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            int y = 20, lw = 110, cw = 280;

            // Nhân viên
            AddLabel("Nhân viên:", 20, y);
            cboEmployee = new ComboBox { Location = new Point(lw + 20, y), Size = new Size(cw, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            this.Controls.Add(cboEmployee);
            if (_existing != null) cboEmployee.Enabled = false;

            y += 40;
            // Ca
            AddLabel("Ca làm việc:", 20, y);
            cboShift = new ComboBox { Location = new Point(lw + 20, y), Size = new Size(cw, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            cboShift.Items.AddRange(new object[] { "MORNING", "AFTERNOON", "FULLDAY", "NIGHT" });
            cboShift.SelectedIndex = 2;
            this.Controls.Add(cboShift);

            y += 40;
            // Giờ vào
            chkCheckIn = new CheckBox { Text = "Giờ vào:", Location = new Point(20, y), AutoSize = true, ForeColor = ThemeColors.Foreground, Checked = true };
            chkCheckIn.CheckedChanged += (s, e) => dtpCheckIn.Enabled = chkCheckIn.Checked;
            this.Controls.Add(chkCheckIn);
            dtpCheckIn = new DateTimePicker { Location = new Point(lw + 20, y), Size = new Size(cw, 30), Format = DateTimePickerFormat.Time, ShowUpDown = true, Value = DateTime.Today.AddHours(7).AddMinutes(30) };
            this.Controls.Add(dtpCheckIn);

            y += 40;
            // Giờ ra
            chkCheckOut = new CheckBox { Text = "Giờ ra:", Location = new Point(20, y), AutoSize = true, ForeColor = ThemeColors.Foreground, Checked = true };
            chkCheckOut.CheckedChanged += (s, e) => dtpCheckOut.Enabled = chkCheckOut.Checked;
            this.Controls.Add(chkCheckOut);
            dtpCheckOut = new DateTimePicker { Location = new Point(lw + 20, y), Size = new Size(cw, 30), Format = DateTimePickerFormat.Time, ShowUpDown = true, Value = DateTime.Today.AddHours(17) };
            this.Controls.Add(dtpCheckOut);

            y += 40;
            // Trạng thái
            AddLabel("Trạng thái:", 20, y);
            cboStatus = new ComboBox { Location = new Point(lw + 20, y), Size = new Size(cw, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            cboStatus.Items.AddRange(new object[] { "Present", "Late", "Absent", "OnLeave", "Holiday" });
            cboStatus.SelectedIndex = 0;
            this.Controls.Add(cboStatus);

            y += 40;
            // Ghi chú
            AddLabel("Ghi chú:", 20, y);
            txtNotes = new TextBox { Location = new Point(lw + 20, y), Size = new Size(cw, 50), Multiline = true, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            this.Controls.Add(txtNotes);

            y += 65;
            // Buttons
            var btnSave = new Button { Text = "💾 Lưu", Location = new Point(lw + 20, y), Size = new Size(120, 35), BackColor = ThemeColors.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            var btnCancel = new Button { Text = "Hủy", Location = new Point(lw + 150, y), Size = new Size(100, 35), BackColor = ThemeColors.MutedForeground, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);

            // Populate existing data
            if (_existing != null)
            {
                cboShift.SelectedItem = _existing.ShiftType ?? "FULLDAY";
                cboStatus.SelectedItem = _existing.Status;
                txtNotes.Text = _existing.Notes ?? "";
                if (_existing.CheckIn.HasValue)
                    dtpCheckIn.Value = DateTime.Today.Add(_existing.CheckIn.Value);
                else { chkCheckIn.Checked = false; dtpCheckIn.Enabled = false; }
                if (_existing.CheckOut.HasValue)
                    dtpCheckOut.Value = DateTime.Today.Add(_existing.CheckOut.Value);
                else { chkCheckOut.Checked = false; dtpCheckOut.Enabled = false; }
            }
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(x, y + 2), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
        }

        private async Task LoadDataAsync()
        {
            if (_existing != null)
            {
                // Hiển thị tên NV đã chọn
                cboEmployee.Items.Add(_existing.EmployeeName ?? "");
                cboEmployee.SelectedIndex = 0;
            }
            else
            {
                // Lấy danh sách NV chưa chấm công ngày này
                var notChecked = (await Program.AttendanceService.GetUncheckedEmployeesAsync(_workDate)).ToList();
                if (!notChecked.Any())
                {
                    FormHelper.ShowError("Tất cả nhân viên đã được chấm công ngày này.");
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }
                cboEmployee.DataSource = notChecked;
                cboEmployee.DisplayMember = "FullName";
                cboEmployee.ValueMember = "EmployeeId";
            }
        }

        private async void BtnSave_Click(object? s, EventArgs e)
        {
            try
            {
                if (_existing == null)
                {
                    // Add new
                    var empId = (int)(cboEmployee.SelectedValue ?? 0);
                    var record = new AttendanceRecord
                    {
                        EmployeeId = empId,
                        WorkDate = _workDate,
                        CheckIn = chkCheckIn.Checked ? dtpCheckIn.Value.TimeOfDay : null,
                        CheckOut = chkCheckOut.Checked ? dtpCheckOut.Value.TimeOfDay : null,
                        ShiftType = cboShift.SelectedItem?.ToString(),
                        Status = cboStatus.SelectedItem?.ToString() ?? "Present",
                        Notes = txtNotes.Text.Trim(),
                        CreatedBy = AppSession.CurrentUser?.UserId ?? 1
                    };
                    var (ok, msg) = await Program.AttendanceService.AddAsync(record);
                    if (!ok) { FormHelper.ShowError(msg); return; }
                    FormHelper.ShowSuccess(msg);
                }
                else
                {
                    // Update
                    _existing.CheckIn = chkCheckIn.Checked ? dtpCheckIn.Value.TimeOfDay : null;
                    _existing.CheckOut = chkCheckOut.Checked ? dtpCheckOut.Value.TimeOfDay : null;
                    _existing.ShiftType = cboShift.SelectedItem?.ToString();
                    _existing.Status = cboStatus.SelectedItem?.ToString() ?? "Present";
                    _existing.Notes = txtNotes.Text.Trim();
                    var (ok, msg) = await Program.AttendanceService.UpdateAsync(_existing);
                    if (!ok) { FormHelper.ShowError(msg); return; }
                    FormHelper.ShowSuccess(msg);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                FormHelper.ShowError($"Lỗi: {ex.Message}");
            }
        }
    }

    // ==================== Dialog: Chấm công hàng loạt ====================
    public class DlgBulkAttendance : Form
    {
        private ComboBox cboDept = null!, cboShift = null!;
        private Label lblInfo = null!;
        private readonly DateTime _workDate;

        public DlgBulkAttendance(DateTime workDate)
        {
            _workDate = workDate;
            InitializeComponent();
            this.Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Chấm Công Hàng Loạt";
            this.Size = new Size(400, 260);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            int y = 20;
            this.Controls.Add(new Label { Text = $"📅 Ngày: {_workDate:dd/MM/yyyy}", Location = new Point(20, y), AutoSize = true, ForeColor = ThemeColors.Success, Font = new Font("Segoe UI", 11F, FontStyle.Bold) });

            y += 40;
            this.Controls.Add(new Label { Text = "Phòng ban:", Location = new Point(20, y + 2), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
            cboDept = new ComboBox { Location = new Point(130, y), Size = new Size(220, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            this.Controls.Add(cboDept);

            y += 40;
            this.Controls.Add(new Label { Text = "Ca:", Location = new Point(20, y + 2), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
            cboShift = new ComboBox { Location = new Point(130, y), Size = new Size(220, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            cboShift.Items.AddRange(new object[] { "MORNING", "AFTERNOON", "FULLDAY", "NIGHT" });
            cboShift.SelectedIndex = 2;
            this.Controls.Add(cboShift);

            y += 40;
            lblInfo = new Label { Text = "", Location = new Point(20, y), AutoSize = true, ForeColor = Color.FromArgb(255, 200, 60) };
            this.Controls.Add(lblInfo);

            y += 30;
            var btnApply = new Button { Text = "✅ Chấm công tất cả", Location = new Point(130, y), Size = new Size(180, 35), BackColor = ThemeColors.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Click += BtnApply_Click;
            this.Controls.Add(btnApply);
        }

        private async Task LoadDataAsync()
        {
            var depts = (await Program.DeptService.GetAllAsync()).ToList();
            depts.Insert(0, new Models.Entities.Department { DepartmentId = 0, DepartmentName = "-- Tất cả phòng ban --" });
            cboDept.DataSource = depts;
            cboDept.DisplayMember = "DepartmentName";
            cboDept.ValueMember = "DepartmentId";

            cboDept.SelectedIndexChanged += async (s, e) => await UpdateInfoAsync();
            await UpdateInfoAsync();
        }

        private async Task UpdateInfoAsync()
        {
            var deptId = cboDept.SelectedValue is int d && d > 0 ? d : (int?)null;
            var notChecked = (await Program.AttendanceService.GetUncheckedEmployeesAsync(_workDate, deptId)).ToList();
            lblInfo.Text = $"⚡ {notChecked.Count} nhân viên chưa chấm công";
        }

        private async void BtnApply_Click(object? s, EventArgs e)
        {
            var deptId = cboDept.SelectedValue is int d && d > 0 ? d : (int?)null;
            var shift = cboShift.SelectedItem?.ToString() ?? "FULLDAY";
            var userId = AppSession.CurrentUser?.UserId ?? 1;

            var (ok, msg, count) = await Program.AttendanceService.BulkMarkAsync(_workDate, deptId, shift, userId);
            if (ok) { FormHelper.ShowSuccess(msg); this.DialogResult = DialogResult.OK; this.Close(); }
            else FormHelper.ShowError(msg);
        }
    }
}
