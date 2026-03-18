using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Salary
{
    /// <summary>
    /// Quản lý tạm ứng lương
    /// </summary>
    public class FrmAdvanceManager : Form
    {
        private DataGridView dgv = null!;
        private Button btnAdd = null!, btnEdit = null!, btnDelete = null!, btnRefresh = null!;
        private NumericUpDown nudMonth = null!, nudYear = null!;
        private Label lblTotal = null!;
        private readonly string _menuCode = "TL_TAMUNG";

        public FrmAdvanceManager()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadDataAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Quản Lý Tạm Ứng";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            var header = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = ThemeColors.Background };
            header.Controls.Add(new Label
            {
                Text = "💸 QUẢN LÝ TẠM ỨNG LƯƠNG", Dock = DockStyle.Left, AutoSize = false,
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

            var lblM = new Label { Text = "Tháng:", AutoSize = true, ForeColor = ThemeColors.Foreground, Margin = new Padding(20, 10, 5, 0) };
            nudMonth = new NumericUpDown
            {
                Size = new Size(60, 30), Minimum = 1, Maximum = 12, Value = DateTime.Now.Month,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, Margin = new Padding(0, 5, 5, 0)
            };
            nudMonth.ValueChanged += async (s, e) => await LoadDataAsync();

            var lblY = new Label { Text = "Năm:", AutoSize = true, ForeColor = ThemeColors.Foreground, Margin = new Padding(5, 10, 5, 0) };
            nudYear = new NumericUpDown
            {
                Size = new Size(80, 30), Minimum = 2020, Maximum = 2099, Value = DateTime.Now.Year,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, Margin = new Padding(0, 5, 0, 0)
            };
            nudYear.ValueChanged += async (s, e) => await LoadDataAsync();

            lblTotal = new Label
            {
                Text = "", AutoSize = true, ForeColor = ThemeColors.Success,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), Margin = new Padding(20, 10, 0, 0)
            };

            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh, lblM, nudMonth, lblY, nudYear, lblTotal });

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
                EnableHeadersVisualStyles = false, RowHeadersVisible = false, BorderStyle = BorderStyle.None
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

        private async Task LoadDataAsync()
        {
            var month = (int)nudMonth.Value;
            var year = (int)nudYear.Value;
            var advances = (await Program.AdvanceService.GetAllAsync(month, year)).ToList();
            dgv.DataSource = advances;

            var hideCols = new[] { "AdvanceId", "EmployeeId", "Month", "Year", "CreatedAt" };
            foreach (var c in hideCols)
                if (dgv.Columns.Contains(c)) dgv.Columns[c].Visible = false;

            var headers = new Dictionary<string, string>
            {
                ["EmployeeCode"] = "Mã NV", ["EmployeeName"] = "Họ Tên",
                ["Amount"] = "Số Tiền", ["AdvanceDate"] = "Ngày Ứng",
                ["Status"] = "Trạng Thái", ["Notes"] = "Ghi Chú"
            };
            foreach (var (col, h) in headers)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].HeaderText = h;

            if (dgv.Columns.Contains("Amount")) dgv.Columns["Amount"].DefaultCellStyle.Format = "#,##0";
            if (dgv.Columns.Contains("AdvanceDate")) dgv.Columns["AdvanceDate"].DefaultCellStyle.Format = "dd/MM/yyyy";

            var total = advances.Sum(a => a.Amount);
            lblTotal.Text = $"Tổng: {total:#,##0} ₫";
        }

        private Advance? GetSelected() => dgv.CurrentRow?.DataBoundItem as Advance;

        private async void BtnAdd_Click(object? s, EventArgs e)
        {
            var dlg = new FrmAdvanceDetail(null, (int)nudMonth.Value, (int)nudYear.Value);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnEdit_Click(object? s, EventArgs e)
        {
            var adv = GetSelected();
            if (adv == null) { FormHelper.ShowError("Vui lòng chọn bản ghi."); return; }
            var dlg = new FrmAdvanceDetail(adv, adv.Month, adv.Year);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnDelete_Click(object? s, EventArgs e)
        {
            var adv = GetSelected();
            if (adv == null) { FormHelper.ShowError("Vui lòng chọn bản ghi."); return; }
            if (!FormHelper.ConfirmDelete("tạm ứng này")) return;
            var (ok, msg) = await Program.AdvanceService.DeleteAsync(adv.AdvanceId);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
            else FormHelper.ShowError(msg);
        }
    }

    /// <summary>
    /// Dialog thêm/sửa tạm ứng
    /// </summary>
    public class FrmAdvanceDetail : Form
    {
        private ComboBox cboEmployee = null!;
        private NumericUpDown nudAmount = null!;
        private DateTimePicker dtpDate = null!;
        private TextBox txtNotes = null!;
        private readonly Advance? _advance;
        private readonly int _month, _year;

        public FrmAdvanceDetail(Advance? advance, int month, int year)
        {
            _advance = advance; _month = month; _year = year;
            InitializeComponent();
            this.Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            this.Text = _advance == null ? "Thêm Tạm Ứng" : "Sửa Tạm Ứng";
            this.Size = new Size(450, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            int y = 20, lx = 20, ix = 160, iw = 240;

            AddLabel("Nhân viên: *", lx, y);
            cboEmployee = new ComboBox
            {
                Location = new Point(ix, y), Size = new Size(iw, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground
            };
            if (_advance != null) cboEmployee.Enabled = false;
            this.Controls.Add(cboEmployee); y += 38;

            AddLabel("Số tiền: *", lx, y);
            nudAmount = new NumericUpDown
            {
                Location = new Point(ix, y), Size = new Size(200, 30),
                Maximum = 500000000, ThousandsSeparator = true, Increment = 500000,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground
            };
            this.Controls.Add(nudAmount); y += 38;

            AddLabel("Ngày ứng:", lx, y);
            dtpDate = new DateTimePicker { Location = new Point(ix, y), Size = new Size(150, 30), Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpDate); y += 38;

            AddLabel("Ghi chú:", lx, y);
            txtNotes = new TextBox
            {
                Location = new Point(ix, y), Size = new Size(iw, 50), Multiline = true,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtNotes); y += 60;

            var btnSave = new Button
            {
                Text = "💾 Lưu", Location = new Point(100, y), Size = new Size(100, 38),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Primary,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            var btnCancel = new Button
            {
                Text = "Hủy", Location = new Point(210, y), Size = new Size(80, 38),
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

            if (_advance != null)
            {
                cboEmployee.SelectedValue = _advance.EmployeeId;
                nudAmount.Value = _advance.Amount;
                dtpDate.Value = _advance.AdvanceDate;
                txtNotes.Text = _advance.Notes;
            }
        }

        private async void BtnSave_Click(object? s, EventArgs e)
        {
            var adv = _advance ?? new Advance();
            adv.EmployeeId = (int)(cboEmployee.SelectedValue ?? 0);
            adv.Amount = nudAmount.Value;
            adv.AdvanceDate = dtpDate.Value;
            adv.Month = _month;
            adv.Year = _year;
            adv.Notes = txtNotes.Text.Trim();

            (bool ok, string msg) result;
            if (_advance == null)
                result = await Program.AdvanceService.CreateAsync(adv);
            else
                result = await Program.AdvanceService.UpdateAsync(adv);

            if (result.ok) { FormHelper.ShowSuccess(result.msg); this.DialogResult = DialogResult.OK; this.Close(); }
            else FormHelper.ShowError(result.msg);
        }
    }
}
