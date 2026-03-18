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
                "EarlyLeave" => Color.FromArgb(255, 160, 50),
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
            cboStatus.Items.AddRange(new object[] { "Present", "Late", "EarlyLeave", "Absent", "OnLeave", "Holiday" });
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

    // ==================== Dialog: Chấm công hàng loạt (inline edit) ====================
    public class DlgBulkAttendance : Form
    {
        private ComboBox cboDept = null!, cboShiftDefault = null!;
        private DataGridView dgvBatch = null!;
        private Label lblInfo = null!;
        private readonly DateTime _workDate;
        private List<Models.Entities.Employee> _employees = new();

        public DlgBulkAttendance(DateTime workDate)
        {
            _workDate = workDate;
            InitializeComponent();
            this.Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeComponent()
        {
            this.Text = $"📋 Chấm Công Hàng Loạt — {_workDate:dd/MM/yyyy}";
            this.Size = new Size(950, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            // === Top filter bar ===
            var filterBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background, Padding = new Padding(10, 8, 10, 0) };

            filterBar.Controls.Add(new Label { Text = "Phòng ban:", Location = new Point(10, 14), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
            cboDept = new ComboBox { Location = new Point(95, 10), Size = new Size(180, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            filterBar.Controls.Add(cboDept);

            filterBar.Controls.Add(new Label { Text = "Ca mặc định:", Location = new Point(295, 14), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
            cboShiftDefault = new ComboBox { Location = new Point(400, 10), Size = new Size(120, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            cboShiftDefault.Items.AddRange(new object[] { "FULLDAY", "MORNING", "AFTERNOON", "NIGHT" });
            cboShiftDefault.SelectedIndex = 0;
            filterBar.Controls.Add(cboShiftDefault);

            var btnFillDefault = new Button { Text = "📝 Điền giờ mặc định", Location = new Point(540, 8), Size = new Size(160, 35), BackColor = Color.FromArgb(80, 80, 120), ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnFillDefault.FlatAppearance.BorderSize = 0;
            btnFillDefault.Click += BtnFillDefault_Click;
            filterBar.Controls.Add(btnFillDefault);

            lblInfo = new Label { Text = "", Location = new Point(720, 14), AutoSize = true, ForeColor = ThemeColors.Success, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            filterBar.Controls.Add(lblInfo);

            this.Controls.Add(filterBar);

            // === Bottom button bar ===
            var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 55, BackColor = ThemeColors.Background, Padding = new Padding(10, 8, 10, 8) };

            var btnSelectAll = new Button { Text = "☑ Chọn tất cả", Location = new Point(10, 8), Size = new Size(130, 38), BackColor = Color.FromArgb(60, 60, 90), ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSelectAll.FlatAppearance.BorderSize = 0;
            btnSelectAll.Click += (s, e) => { foreach (DataGridViewRow r in dgvBatch.Rows) r.Cells["Select"].Value = true; };
            bottomBar.Controls.Add(btnSelectAll);

            var btnDeselectAll = new Button { Text = "☐ Bỏ chọn", Location = new Point(150, 8), Size = new Size(110, 38), BackColor = Color.FromArgb(60, 60, 90), ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnDeselectAll.FlatAppearance.BorderSize = 0;
            btnDeselectAll.Click += (s, e) => { foreach (DataGridViewRow r in dgvBatch.Rows) r.Cells["Select"].Value = false; };
            bottomBar.Controls.Add(btnDeselectAll);

            var btnSave = new Button { Text = "✅ Lưu chấm công", Size = new Size(170, 38), BackColor = ThemeColors.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Location = new Point(this.ClientSize.Width - 190, 8);
            btnSave.Click += BtnSave_Click;
            bottomBar.Controls.Add(btnSave);

            var btnCancel = new Button { Text = "Hủy", Size = new Size(100, 38), BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderColor = ThemeColors.Border;
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Location = new Point(this.ClientSize.Width - 300, 8);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            bottomBar.Controls.Add(btnCancel);

            this.Controls.Add(bottomBar);

            // === DataGridView inline edit ===
            dgvBatch = new DataGridView
            {
                Dock = DockStyle.Fill, AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = ThemeColors.Background, GridColor = ThemeColors.Border,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground,
                    SelectionBackColor = Color.FromArgb(60, 60, 90), Font = new Font("Segoe UI", 9.5F)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = ThemeColors.Surface, ForeColor = ThemeColors.MutedForeground,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
                },
                EnableHeadersVisualStyles = false, RowHeadersVisible = false, BorderStyle = BorderStyle.None,
                EditMode = DataGridViewEditMode.EditOnEnter
            };

            // Columns
            var colSelect = new DataGridViewCheckBoxColumn { Name = "Select", HeaderText = "☑", Width = 35, FillWeight = 20 };
            dgvBatch.Columns.Add(colSelect);

            dgvBatch.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmpCode", HeaderText = "Mã NV", ReadOnly = true, FillWeight = 50 });
            dgvBatch.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmpName", HeaderText = "Họ Tên", ReadOnly = true, FillWeight = 80 });
            dgvBatch.Columns.Add(new DataGridViewTextBoxColumn { Name = "DeptName", HeaderText = "Phòng Ban", ReadOnly = true, FillWeight = 70 });

            var colShift = new DataGridViewComboBoxColumn { Name = "Shift", HeaderText = "Ca", FillWeight = 60 };
            colShift.Items.AddRange("FULLDAY", "MORNING", "AFTERNOON", "NIGHT");
            dgvBatch.Columns.Add(colShift);

            dgvBatch.Columns.Add(new DataGridViewTextBoxColumn { Name = "CheckIn", HeaderText = "Giờ Vào (HH:mm)", FillWeight = 60 });
            dgvBatch.Columns.Add(new DataGridViewTextBoxColumn { Name = "CheckOut", HeaderText = "Giờ Ra (HH:mm)", FillWeight = 60 });

            var colStatus = new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng Thái", ReadOnly = true, FillWeight = 60 };
            dgvBatch.Columns.Add(colStatus);

            dgvBatch.Columns.Add(new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "Ghi Chú", FillWeight = 70 });

            // Hidden column for EmployeeId
            dgvBatch.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmpId", Visible = false });

            dgvBatch.CellEndEdit += DgvBatch_CellEndEdit;
            dgvBatch.CellFormatting += DgvBatch_CellFormatting;

            // Dock order: Fill first
            this.Controls.Add(dgvBatch);
        }

        private async Task LoadDataAsync()
        {
            var depts = (await Program.DeptService.GetAllAsync()).ToList();
            depts.Insert(0, new Models.Entities.Department { DepartmentId = 0, DepartmentName = "-- Tất cả phòng ban --" });
            cboDept.DataSource = depts;
            cboDept.DisplayMember = "DepartmentName";
            cboDept.ValueMember = "DepartmentId";

            cboDept.SelectedIndexChanged += async (s, e) => await LoadEmployeesAsync();
            await LoadEmployeesAsync();
        }

        private async Task LoadEmployeesAsync()
        {
            var deptId = cboDept.SelectedValue is int d && d > 0 ? d : (int?)null;
            _employees = (await Program.AttendanceService.GetUncheckedEmployeesAsync(_workDate, deptId)).ToList();

            dgvBatch.Rows.Clear();
            foreach (var emp in _employees)
            {
                var rowIdx = dgvBatch.Rows.Add();
                var row = dgvBatch.Rows[rowIdx];
                row.Cells["Select"].Value = true;
                row.Cells["EmpCode"].Value = emp.EmployeeCode;
                row.Cells["EmpName"].Value = emp.FullName;
                row.Cells["DeptName"].Value = emp.DepartmentName;
                row.Cells["Shift"].Value = cboShiftDefault.SelectedItem?.ToString() ?? "FULLDAY";
                row.Cells["CheckIn"].Value = "";
                row.Cells["CheckOut"].Value = "";
                row.Cells["Status"].Value = "Present";
                row.Cells["Notes"].Value = "";
                row.Cells["EmpId"].Value = emp.EmployeeId;
            }

            lblInfo.Text = $"⚡ {_employees.Count} nhân viên chưa chấm công";
        }

        /// <summary>
        /// Tự động detect trạng thái khi user nhập giờ vào/ra
        /// </summary>
        private void DgvBatch_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (dgvBatch.Columns[e.ColumnIndex].Name is "CheckIn" or "CheckOut" or "Shift")
            {
                AutoDetectStatus(e.RowIndex);
            }
        }

        private void AutoDetectStatus(int rowIndex)
        {
            var row = dgvBatch.Rows[rowIndex];
            var checkInStr = row.Cells["CheckIn"].Value?.ToString()?.Trim() ?? "";
            var checkOutStr = row.Cells["CheckOut"].Value?.ToString()?.Trim() ?? "";
            var shift = row.Cells["Shift"].Value?.ToString() ?? "FULLDAY";

            if (string.IsNullOrEmpty(checkInStr))
            {
                row.Cells["Status"].Value = "Present";
                return;
            }

            if (!TimeSpan.TryParse(checkInStr, out var checkIn))
            {
                row.Cells["Status"].Value = "?";
                return;
            }

            TimeSpan? checkOut = null;
            if (!string.IsNullOrEmpty(checkOutStr) && TimeSpan.TryParse(checkOutStr, out var co))
                checkOut = co;

            // Lấy giờ ca
            var shiftStart = GetShiftStart(shift);
            var shiftEnd = GetShiftEnd(shift);

            bool isLate = checkIn > shiftStart.Add(TimeSpan.FromMinutes(15));
            bool isEarlyLeave = false;
            if (checkOut.HasValue)
            {
                if (shift == "NIGHT")
                {
                    var normCO = checkOut.Value < checkIn ? checkOut.Value.Add(TimeSpan.FromHours(24)) : checkOut.Value;
                    var normEnd = shiftEnd.Add(TimeSpan.FromHours(24));
                    isEarlyLeave = normCO < normEnd.Subtract(TimeSpan.FromMinutes(15));
                }
                else
                {
                    isEarlyLeave = checkOut.Value < shiftEnd.Subtract(TimeSpan.FromMinutes(15));
                }
            }

            if (isLate) row.Cells["Status"].Value = "Late";
            else if (isEarlyLeave) row.Cells["Status"].Value = "EarlyLeave";
            else row.Cells["Status"].Value = "Present";
        }

        private static TimeSpan GetShiftStart(string shift) => shift switch
        {
            "MORNING" or "FULLDAY" => new TimeSpan(7, 30, 0),
            "AFTERNOON" => new TimeSpan(13, 0, 0),
            "NIGHT" => new TimeSpan(22, 0, 0),
            _ => new TimeSpan(8, 0, 0)
        };

        private static TimeSpan GetShiftEnd(string shift) => shift switch
        {
            "MORNING" => new TimeSpan(11, 30, 0),
            "AFTERNOON" or "FULLDAY" => new TimeSpan(17, 0, 0),
            "NIGHT" => new TimeSpan(6, 0, 0),
            _ => new TimeSpan(17, 0, 0)
        };

        private void DgvBatch_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvBatch.Columns[e.ColumnIndex].Name != "Status" || e.Value == null) return;
            e.CellStyle!.ForeColor = e.Value.ToString() switch
            {
                "Present" => ThemeColors.Success,
                "Late" => Color.FromArgb(255, 200, 60),
                "EarlyLeave" => Color.FromArgb(255, 160, 50),
                _ => ThemeColors.Foreground
            };
            e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        }

        private void BtnFillDefault_Click(object? s, EventArgs e)
        {
            var shift = cboShiftDefault.SelectedItem?.ToString() ?? "FULLDAY";
            var start = GetShiftStart(shift);
            var end = GetShiftEnd(shift);

            foreach (DataGridViewRow row in dgvBatch.Rows)
            {
                row.Cells["Shift"].Value = shift;
                row.Cells["CheckIn"].Value = $"{start.Hours:D2}:{start.Minutes:D2}";
                row.Cells["CheckOut"].Value = $"{end.Hours:D2}:{end.Minutes:D2}";
                row.Cells["Status"].Value = "Present";
            }
        }

        private async void BtnSave_Click(object? s, EventArgs e)
        {
            var records = new List<AttendanceRecord>();
            var userId = AppSession.CurrentUser?.UserId ?? 1;

            foreach (DataGridViewRow row in dgvBatch.Rows)
            {
                var selected = row.Cells["Select"].Value is true;
                if (!selected) continue;

                var empId = Convert.ToInt32(row.Cells["EmpId"].Value);
                var shift = row.Cells["Shift"].Value?.ToString() ?? "FULLDAY";
                var ciStr = row.Cells["CheckIn"].Value?.ToString()?.Trim() ?? "";
                var coStr = row.Cells["CheckOut"].Value?.ToString()?.Trim() ?? "";
                var status = row.Cells["Status"].Value?.ToString() ?? "Present";
                var notes = row.Cells["Notes"].Value?.ToString() ?? "";

                TimeSpan? checkIn = TimeSpan.TryParse(ciStr, out var ci) ? ci : null;
                TimeSpan? checkOut = TimeSpan.TryParse(coStr, out var co) ? co : null;

                records.Add(new AttendanceRecord
                {
                    EmployeeId = empId,
                    WorkDate = _workDate,
                    ShiftType = shift,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    Status = status == "?" ? "Present" : status,
                    Notes = notes,
                    CreatedBy = userId
                });
            }

            if (records.Count == 0)
            {
                FormHelper.ShowError("Chưa chọn nhân viên nào để chấm công.");
                return;
            }

            var (ok, msg, count) = await Program.AttendanceService.BulkInsertWithDetailsAsync(records);
            if (ok)
            {
                FormHelper.ShowSuccess(msg);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else FormHelper.ShowError(msg);
        }
    }
}
