using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Employee
{
    /// <summary>
    /// Quản lý biến động nhân sự: khen thưởng, kỷ luật, thăng chức, điều chuyển
    /// </summary>
    public class FrmEmployeeEvents : Form
    {
        private DataGridView dgv = null!;
        private Button btnAdd = null!, btnEdit = null!, btnDelete = null!, btnRefresh = null!;
        private ComboBox cboFilter = null!;
        private readonly string _menuCode = "NS_BIENDONG";

        private static readonly Dictionary<string, string> EventTypes = new()
        {
            ["REWARD"] = "🏆 Khen thưởng",
            ["DISCIPLINE"] = "⚠️ Kỷ luật",
            ["PROMOTION"] = "📈 Thăng chức",
            ["TRANSFER"] = "🔄 Điều chuyển"
        };

        public FrmEmployeeEvents()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadDataAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Biến Động Nhân Sự";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            var header = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = ThemeColors.Background };
            header.Controls.Add(new Label
            {
                Text = "📋 BIẾN ĐỘNG NHÂN SỰ", Dock = DockStyle.Left, AutoSize = false,
                Width = 400, TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Padding = new Padding(15, 0, 0, 0)
            });

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background,
                Padding = new Padding(8, 8, 0, 0)
            };
            btnAdd = CreateBtn("➕ Thêm", "Add", ThemeColors.Primary);
            btnEdit = CreateBtn("✏️ Sửa", "Edit", ThemeColors.Success);
            btnDelete = CreateBtn("🗑️ Xóa", "Delete", ThemeColors.Error);
            btnRefresh = CreateBtn("🔄", "", ThemeColors.MutedForeground);
            btnRefresh.Size = new Size(50, 35);

            // Filter by type
            var lblFilter = new Label
            {
                Text = "Lọc:", AutoSize = true, ForeColor = ThemeColors.Foreground,
                Margin = new Padding(20, 10, 5, 0)
            };
            cboFilter = new ComboBox
            {
                Size = new Size(180, 30), DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 5, 0, 0)
            };
            cboFilter.Items.Add("-- Tất cả --");
            foreach (var kv in EventTypes) cboFilter.Items.Add(kv.Value);
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += async (s, e) => await LoadDataAsync();

            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh, lblFilter, cboFilter });

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

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

            this.Controls.Add(dgv);
            this.Controls.Add(toolbar);
            this.Controls.Add(header);
        }

        private Button CreateBtn(string text, string tag, Color c)
        {
            var b = new Button
            {
                Text = text, Tag = tag, Size = new Size(110, 35), FlatStyle = FlatStyle.Flat,
                BackColor = c, ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 8, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private string? GetFilterType()
        {
            if (cboFilter.SelectedIndex <= 0) return null;
            return EventTypes.Keys.ElementAt(cboFilter.SelectedIndex - 1);
        }

        private async Task LoadDataAsync()
        {
            var events = (await Program.EmployeeEventService.GetAllAsync(eventType: GetFilterType())).ToList();
            dgv.DataSource = events;

            var hideCols = new[] { "EventId", "EmployeeId", "CreatedAt" };
            foreach (var c in hideCols)
                if (dgv.Columns.Contains(c)) dgv.Columns[c].Visible = false;

            var headers = new Dictionary<string, string>
            {
                ["EmployeeCode"] = "Mã NV", ["EmployeeName"] = "Họ Tên",
                ["EventType"] = "Loại", ["EventDate"] = "Ngày",
                ["Description"] = "Mô Tả", ["Amount"] = "Số Tiền", ["CreatedBy"] = "Người Tạo"
            };
            foreach (var (col, h) in headers)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].HeaderText = h;

            if (dgv.Columns.Contains("EventDate")) dgv.Columns["EventDate"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (dgv.Columns.Contains("Amount")) dgv.Columns["Amount"].DefaultCellStyle.Format = "#,##0";

            // Map event type codes to display names
            foreach (DataGridViewRow row in dgv.Rows)
            {
                var type = row.Cells["EventType"]?.Value?.ToString();
                if (type != null && EventTypes.ContainsKey(type))
                    row.Cells["EventType"].Value = EventTypes[type];
            }
        }

        private EmployeeEvent? GetSelected() => dgv.CurrentRow?.DataBoundItem as EmployeeEvent;

        private async void BtnAdd_Click(object? s, EventArgs e)
        {
            var dlg = new FrmEventDetail(null);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnEdit_Click(object? s, EventArgs e)
        {
            var ev = GetSelected();
            if (ev == null) { FormHelper.ShowError("Vui lòng chọn sự kiện."); return; }
            var dlg = new FrmEventDetail(ev);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnDelete_Click(object? s, EventArgs e)
        {
            var ev = GetSelected();
            if (ev == null) { FormHelper.ShowError("Vui lòng chọn sự kiện."); return; }
            if (!FormHelper.ConfirmDelete("sự kiện này")) return;
            var (ok, msg) = await Program.EmployeeEventService.DeleteAsync(ev.EventId);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
            else FormHelper.ShowError(msg);
        }
    }

    /// <summary>
    /// Dialog thêm/sửa sự kiện nhân sự
    /// </summary>
    public class FrmEventDetail : Form
    {
        private ComboBox cboEmployee = null!, cboType = null!;
        private DateTimePicker dtpDate = null!;
        private TextBox txtDesc = null!;
        private NumericUpDown nudAmount = null!;
        private readonly EmployeeEvent? _event;

        public FrmEventDetail(EmployeeEvent? ev)
        {
            _event = ev;
            InitializeComponent();
            this.Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            this.Text = _event == null ? "Thêm Sự Kiện Mới" : "Sửa Sự Kiện";
            this.Size = new Size(480, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            int y = 20, lx = 20, ix = 170, iw = 260;

            AddLabel("Nhân viên: *", lx, y);
            cboEmployee = new ComboBox
            {
                Location = new Point(ix, y), Size = new Size(iw, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground
            };
            if (_event != null) cboEmployee.Enabled = false;
            this.Controls.Add(cboEmployee); y += 38;

            AddLabel("Loại sự kiện: *", lx, y);
            cboType = new ComboBox
            {
                Location = new Point(ix, y), Size = new Size(iw, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground
            };
            cboType.Items.AddRange(new object[] { "REWARD - Khen thưởng", "DISCIPLINE - Kỷ luật", "PROMOTION - Thăng chức", "TRANSFER - Điều chuyển" });
            this.Controls.Add(cboType); y += 38;

            AddLabel("Ngày:", lx, y);
            dtpDate = new DateTimePicker { Location = new Point(ix, y), Size = new Size(150, 30), Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpDate); y += 38;

            AddLabel("Số tiền (nếu có):", lx, y);
            nudAmount = new NumericUpDown
            {
                Location = new Point(ix, y), Size = new Size(180, 30),
                Maximum = 500000000, ThousandsSeparator = true,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground
            };
            this.Controls.Add(nudAmount); y += 38;

            AddLabel("Mô tả: *", lx, y);
            txtDesc = new TextBox
            {
                Location = new Point(ix, y), Size = new Size(iw, 60), Multiline = true,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtDesc); y += 70;

            var btnSave = new Button
            {
                Text = "💾 Lưu", Location = new Point(120, y), Size = new Size(100, 38),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Primary,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            var btnCancel = new Button
            {
                Text = "Hủy", Location = new Point(230, y), Size = new Size(80, 38),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.MutedForeground, ForeColor = ThemeColors.Foreground
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label
            {
                Text = text, Location = new Point(x, y + 3), AutoSize = true,
                ForeColor = ThemeColors.MutedForeground
            });
        }

        private async Task LoadAsync()
        {
            var emps = (await Program.EmployeeService.GetListAsync(1, 500, "", null, true)).Items.ToList();
            cboEmployee.DataSource = emps;
            cboEmployee.DisplayMember = "FullName";
            cboEmployee.ValueMember = "EmployeeId";

            if (_event != null)
            {
                cboEmployee.SelectedValue = _event.EmployeeId;
                var typeIdx = _event.EventType switch
                {
                    "REWARD" => 0, "DISCIPLINE" => 1, "PROMOTION" => 2, "TRANSFER" => 3, _ => -1
                };
                cboType.SelectedIndex = typeIdx;
                dtpDate.Value = _event.EventDate;
                nudAmount.Value = _event.Amount;
                txtDesc.Text = _event.Description;
            }
        }

        private async void BtnSave_Click(object? s, EventArgs e)
        {
            var ev = _event ?? new EmployeeEvent();
            ev.EmployeeId = (int)(cboEmployee.SelectedValue ?? 0);
            ev.EventType = cboType.SelectedItem?.ToString()?.Split(" - ")[0] ?? "";
            ev.EventDate = dtpDate.Value;
            ev.Amount = nudAmount.Value;
            ev.Description = txtDesc.Text.Trim();
            ev.CreatedBy = AppSession.DisplayName;

            (bool ok, string msg) result;
            if (_event == null)
                result = await Program.EmployeeEventService.CreateAsync(ev);
            else
                result = await Program.EmployeeEventService.UpdateAsync(ev);

            if (result.ok) { FormHelper.ShowSuccess(result.msg); this.DialogResult = DialogResult.OK; this.Close(); }
            else FormHelper.ShowError(result.msg);
        }
    }
}
