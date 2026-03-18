using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Attendance
{
    public class FrmLeaveRequest : Form
    {
        private DataGridView dgv = null!;
        private Button btnAdd = null!, btnApprove = null!, btnReject = null!, btnDelete = null!, btnRefresh = null!;
        private ComboBox cboStatus = null!;
        private Label lblCount = null!;
        private readonly string _menuCode = "CC_PHEP";
        private int _page = 1;
        private const int PageSize = 50;

        public FrmLeaveRequest()
        {
            InitializeComponent();
            this.Load += async (s, e) =>
            {
                FormHelper.ApplyPermissions(this, _menuCode);
                await LoadDataAsync();
            };
        }

        private void InitializeComponent()
        {
            this.Text = "Đơn Xin Nghỉ Phép";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // === Toolbar ===
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background,
                Padding = new Padding(8, 8, 0, 0)
            };
            btnAdd = CreateBtn("➕ Tạo đơn", "Add", ThemeColors.Primary);
            btnApprove = CreateBtn("✅ Duyệt", "Edit", ThemeColors.Success);
            btnReject = CreateBtn("❌ Từ chối", "Edit", Color.FromArgb(200, 130, 50));
            btnDelete = CreateBtn("🗑️ Xóa", "Delete", ThemeColors.Error);
            btnRefresh = CreateBtn("🔄", "", ThemeColors.MutedForeground);
            btnRefresh.Size = new Size(50, 35);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnApprove, btnReject, btnDelete, btnRefresh });


            btnAdd.Click += BtnAdd_Click;
            btnApprove.Click += BtnApprove_Click;
            btnReject.Click += BtnReject_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            // === Filter bar ===
            var filterBar = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = ThemeColors.Background };


            filterBar.Controls.Add(new Label { Text = "Trạng thái:", Location = new Point(10, 12), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
            cboStatus = new ComboBox
            {
                Location = new Point(95, 8), Size = new Size(150, 30), DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat
            };
            cboStatus.Items.AddRange(new object[] { "-- Tất cả --", "Pending", "Approved", "Rejected" });
            cboStatus.SelectedIndex = 0;
            cboStatus.SelectedIndexChanged += async (s, e) => { _page = 1; await LoadDataAsync(); };
            filterBar.Controls.Add(cboStatus);

            lblCount = new Label
            {
                Text = "", Location = new Point(270, 12), AutoSize = true,
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
                Text = text, Tag = tag, Size = new Size(120, 35), FlatStyle = FlatStyle.Flat,
                BackColor = c, ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 8, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private async Task LoadDataAsync()
        {
            var status = cboStatus.SelectedIndex > 0 ? cboStatus.SelectedItem?.ToString() : null;
            var result = await Program.LeaveService.GetListAsync(null, status, _page, PageSize);

            dgv.DataSource = null;
            dgv.DataSource = result.Items.ToList();

            // Ẩn cột
            var hideCols = new[] { "LeaveId", "EmployeeId", "ApprovedBy", "CreatedAt" };
            foreach (var col in hideCols)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].Visible = false;

            // Đặt tên cột
            var headers = new Dictionary<string, string>
            {
                ["EmployeeName"] = "Nhân Viên", ["LeaveType"] = "Loại Phép",
                ["StartDate"] = "Từ Ngày", ["EndDate"] = "Đến Ngày", ["TotalDays"] = "Số Ngày",
                ["Reason"] = "Lý Do", ["Status"] = "Trạng Thái",
                ["ApprovedByName"] = "Người Duyệt", ["ApprovedAt"] = "Ngày Duyệt"
            };
            foreach (var (col, header) in headers)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].HeaderText = header;

            lblCount.Text = $"📊 {result.TotalCount} đơn";
        }

        private void Dgv_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgv.Columns[e.ColumnIndex].Name != "Status" || e.Value == null) return;

            var status = e.Value.ToString();
            e.CellStyle!.ForeColor = status switch
            {
                "Pending" => Color.FromArgb(255, 200, 60),
                "Approved" => ThemeColors.Success,
                "Rejected" => Color.FromArgb(220, 70, 70),
                _ => Color.White
            };
            e.CellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }

        private LeaveRequest? GetSelected() => dgv.CurrentRow?.DataBoundItem as LeaveRequest;

        // ==================== CRUD ====================

        private async void BtnAdd_Click(object? s, EventArgs e)
        {
            var dlg = new DlgLeaveEntry();
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnApprove_Click(object? s, EventArgs e)
        {
            var leave = GetSelected();
            if (leave == null) { FormHelper.ShowError("Vui lòng chọn đơn."); return; }
            var userId = AppSession.CurrentUser?.UserId ?? 1;
            var (ok, msg) = await Program.LeaveService.ApproveAsync(leave.LeaveId, userId);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
            else FormHelper.ShowError(msg);
        }

        private async void BtnReject_Click(object? s, EventArgs e)
        {
            var leave = GetSelected();
            if (leave == null) { FormHelper.ShowError("Vui lòng chọn đơn."); return; }
            var userId = AppSession.CurrentUser?.UserId ?? 1;
            var (ok, msg) = await Program.LeaveService.RejectAsync(leave.LeaveId, userId);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
            else FormHelper.ShowError(msg);
        }

        private async void BtnDelete_Click(object? s, EventArgs e)
        {
            var leave = GetSelected();
            if (leave == null) { FormHelper.ShowError("Vui lòng chọn đơn."); return; }
            if (!FormHelper.ConfirmDelete("đơn nghỉ phép này")) return;
            var (ok, msg) = await Program.LeaveService.DeleteAsync(leave.LeaveId);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
            else FormHelper.ShowError(msg);
        }
    }

    // ==================== Dialog: Tạo đơn nghỉ phép ====================
    public class DlgLeaveEntry : Form
    {
        private ComboBox cboEmployee = null!, cboLeaveType = null!;
        private DateTimePicker dtpStart = null!, dtpEnd = null!;
        private TextBox txtReason = null!;
        private Label lblDays = null!;

        public DlgLeaveEntry()
        {
            InitializeComponent();
            this.Load += async (s, e) => await LoadDataAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Tạo Đơn Nghỉ Phép";
            this.Size = new Size(450, 380);
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

            y += 40;
            // Loại phép
            AddLabel("Loại phép:", 20, y);
            cboLeaveType = new ComboBox { Location = new Point(lw + 20, y), Size = new Size(cw, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            cboLeaveType.Items.AddRange(new object[] { "ANNUAL", "SICK", "UNPAID", "MATERNITY", "BEREAVEMENT", "OTHER" });
            cboLeaveType.SelectedIndex = 0;
            this.Controls.Add(cboLeaveType);

            y += 40;
            // Từ ngày
            AddLabel("Từ ngày:", 20, y);
            dtpStart = new DateTimePicker { Location = new Point(lw + 20, y), Size = new Size(cw, 30), Format = DateTimePickerFormat.Short };
            dtpStart.ValueChanged += (s, e) => UpdateDays();
            this.Controls.Add(dtpStart);

            y += 40;
            // Đến ngày
            AddLabel("Đến ngày:", 20, y);
            dtpEnd = new DateTimePicker { Location = new Point(lw + 20, y), Size = new Size(cw, 30), Format = DateTimePickerFormat.Short };
            dtpEnd.ValueChanged += (s, e) => UpdateDays();
            this.Controls.Add(dtpEnd);

            y += 40;
            // Số ngày
            lblDays = new Label { Text = "📅 1 ngày làm việc", Location = new Point(lw + 20, y), AutoSize = true, ForeColor = ThemeColors.Success, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            this.Controls.Add(lblDays);

            y += 35;
            // Lý do
            AddLabel("Lý do:", 20, y);
            txtReason = new TextBox { Location = new Point(lw + 20, y), Size = new Size(cw, 50), Multiline = true, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            this.Controls.Add(txtReason);

            y += 65;
            // Buttons
            var btnSave = new Button { Text = "💾 Tạo đơn", Location = new Point(lw + 20, y), Size = new Size(120, 35), BackColor = ThemeColors.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            var btnCancel = new Button { Text = "Hủy", Location = new Point(lw + 150, y), Size = new Size(100, 35), BackColor = ThemeColors.MutedForeground, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(x, y + 2), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
        }

        private void UpdateDays()
        {
            int count = 0;
            for (var d = dtpStart.Value.Date; d <= dtpEnd.Value.Date; d = d.AddDays(1))
            {
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    count++;
            }
            lblDays.Text = $"📅 {count} ngày làm việc";
        }

        private async Task LoadDataAsync()
        {
            // Lấy danh sách NV active
            var result = await Program.EmployeeService.GetListAsync(1, 1000, null, null, true);
            cboEmployee.DataSource = result.Items;
            cboEmployee.DisplayMember = "FullName";
            cboEmployee.ValueMember = "EmployeeId";
        }

        private async void BtnSave_Click(object? s, EventArgs e)
        {
            try
            {
                var empId = (int)(cboEmployee.SelectedValue ?? 0);
                var req = new LeaveRequest
                {
                    EmployeeId = empId,
                    LeaveType = cboLeaveType.SelectedItem?.ToString() ?? "ANNUAL",
                    StartDate = dtpStart.Value.Date,
                    EndDate = dtpEnd.Value.Date,
                    Reason = txtReason.Text.Trim()
                };
                var (ok, msg, _) = await Program.LeaveService.CreateAsync(req);
                if (!ok) { FormHelper.ShowError(msg); return; }
                FormHelper.ShowSuccess(msg);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                FormHelper.ShowError($"Lỗi: {ex.Message}");
            }
        }
    }
}
