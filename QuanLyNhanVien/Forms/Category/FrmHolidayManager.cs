using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Category
{
    /// <summary>
    /// MISS-3: Quản lý ngày lễ — CRUD holidays từ database
    /// </summary>
    public class FrmHolidayManager : Form
    {
        private DataGridView dgvHolidays = null!;
        private Button btnAdd = null!, btnEdit = null!, btnDelete = null!, btnRefresh = null!;
        private ComboBox cboYear = null!;
        private readonly string _menuCode = "DM_NGAYLE";

        public FrmHolidayManager()
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
            this.Text = "Quản lý ngày lễ";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // Header bar
            var header = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Surface, Padding = new Padding(16, 0, 8, 0) };
            header.Paint += (s, e) =>
            {
                using var pen = new Pen(ThemeColors.Border, 1);
                e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
            };
            header.Controls.Add(new Label
            {
                Text = "📅 Quản Lý Ngày Lễ", Dock = DockStyle.Left, Width = 200,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            });

            // Year filter + buttons
            var filterPanel = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 340, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 9, 5, 0) };
            btnRefresh = SmallBtn("🔄", ThemeColors.MutedForeground);
            btnDelete = SmallBtn("🗑️", ThemeColors.Error);
            btnEdit = SmallBtn("✏️", ThemeColors.Success);
            btnAdd = SmallBtn("➕", ThemeColors.Primary);

            cboYear = new ComboBox
            {
                Width = 80, Height = 28, DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground,
                Margin = new Padding(3, 0, 0, 0)
            };
            for (int y = DateTime.Now.Year - 1; y <= DateTime.Now.Year + 2; y++) cboYear.Items.Add(y);
            cboYear.SelectedItem = DateTime.Now.Year;
            cboYear.SelectedIndexChanged += async (s, e) => await LoadDataAsync();

            filterPanel.Controls.AddRange(new Control[] { btnRefresh, btnDelete, btnEdit, btnAdd, cboYear });
            header.Controls.Add(filterPanel);

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            // DataGridView
            dgvHolidays = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            ThemeColors.StyleDataGridView(dgvHolidays);
            dgvHolidays.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };

            this.Controls.Add(dgvHolidays);
            this.Controls.Add(header);
        }

        private Button SmallBtn(string text, Color c)
        {
            var b = new Button
            {
                Text = text, Size = new Size(32, 30), FlatStyle = FlatStyle.Flat,
                BackColor = c, ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(3, 0, 0, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private async Task LoadDataAsync()
        {
            int? year = cboYear.SelectedItem as int?;
            var data = (await Program.HolidayRepo.GetAllAsync(year)).ToList();
            dgvHolidays.DataSource = null;
            dgvHolidays.DataSource = data;

            if (dgvHolidays.Columns.Contains("HolidayId")) dgvHolidays.Columns["HolidayId"].Visible = false;
            if (dgvHolidays.Columns.Contains("IsActive")) dgvHolidays.Columns["IsActive"].Visible = false;
            if (dgvHolidays.Columns.Contains("HolidayName")) dgvHolidays.Columns["HolidayName"].HeaderText = "Tên ngày lễ";
            if (dgvHolidays.Columns.Contains("HolidayDate")) dgvHolidays.Columns["HolidayDate"].HeaderText = "Ngày";
            if (dgvHolidays.Columns.Contains("Year")) dgvHolidays.Columns["Year"].HeaderText = "Năm";
            if (dgvHolidays.Columns.Contains("IsRecurring")) dgvHolidays.Columns["IsRecurring"].HeaderText = "Lặp lại";
            if (dgvHolidays.Columns.Contains("Notes")) dgvHolidays.Columns["Notes"].HeaderText = "Ghi chú";
        }

        private async void BtnAdd_Click(object? s, EventArgs e)
        {
            var dlg = new FrmHolidayDetail(null);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnEdit_Click(object? s, EventArgs e)
        {
            var item = dgvHolidays.CurrentRow?.DataBoundItem as Holiday;
            if (item == null) return;
            var dlg = new FrmHolidayDetail(item);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnDelete_Click(object? s, EventArgs e)
        {
            var item = dgvHolidays.CurrentRow?.DataBoundItem as Holiday;
            if (item == null) return;
            if (!FormHelper.ConfirmDelete(item.HolidayName)) return;
            await Program.HolidayRepo.DeleteAsync(item.HolidayId);
            FormHelper.ShowSuccess("Đã xóa ngày lễ.");
            await LoadDataAsync();
        }
    }

    // === Holiday Detail Dialog ===
    public class FrmHolidayDetail : Form
    {
        private TextBox txtName = null!, txtNotes = null!;
        private DateTimePicker dtpDate = null!;
        private NumericUpDown nudYear = null!;
        private CheckBox chkRecurring = null!;
        private Holiday? _holiday;

        public FrmHolidayDetail(Holiday? holiday)
        {
            _holiday = holiday;
            this.Text = _holiday == null ? "Thêm ngày lễ" : "Sửa ngày lễ";
            this.Size = new Size(420, 340);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            var y = 15;
            Lbl("Tên ngày lễ:", 20, y);
            txtName = Txt(150, y, 220); y += 40;

            Lbl("Ngày:", 20, y);
            dtpDate = new DateTimePicker { Location = new Point(150, y), Size = new Size(220, 28), Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpDate); y += 40;

            Lbl("Năm:", 20, y);
            nudYear = new NumericUpDown
            {
                Location = new Point(150, y), Size = new Size(100, 28),
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground,
                Minimum = 2020, Maximum = 2040, Value = DateTime.Now.Year
            };
            this.Controls.Add(nudYear); y += 40;

            chkRecurring = new CheckBox
            {
                Text = "Lặp lại hàng năm (dương lịch)", Location = new Point(150, y),
                AutoSize = true, ForeColor = ThemeColors.Foreground
            };
            this.Controls.Add(chkRecurring); y += 40;

            Lbl("Ghi chú:", 20, y);
            txtNotes = Txt(150, y, 220); y += 50;

            var btnSave = ThemeColors.CreatePrimaryButton("💾 Lưu", 110, 38);
            btnSave.Location = new Point(150, y);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            if (_holiday != null)
            {
                txtName.Text = _holiday.HolidayName;
                dtpDate.Value = _holiday.HolidayDate;
                nudYear.Value = _holiday.Year;
                chkRecurring.Checked = _holiday.IsRecurring;
                txtNotes.Text = _holiday.Notes ?? "";
            }
        }

        private void Lbl(string t, int x, int y)
        {
            this.Controls.Add(new Label { Text = t, Location = new Point(x, y + 3), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
        }

        private TextBox Txt(int x, int y, int w)
        {
            var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 28), BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(tb); return tb;
        }

        private async void BtnSave_Click(object? s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                FormHelper.ShowError("Vui lòng nhập tên ngày lễ.");
                return;
            }

            if (_holiday == null)
            {
                var h = new Holiday
                {
                    HolidayName = txtName.Text.Trim(),
                    HolidayDate = dtpDate.Value.Date,
                    Year = (int)nudYear.Value,
                    IsRecurring = chkRecurring.Checked,
                    Notes = txtNotes.Text.Trim()
                };
                await Program.HolidayRepo.InsertAsync(h);
                FormHelper.ShowSuccess("Thêm ngày lễ thành công!");
            }
            else
            {
                _holiday.HolidayName = txtName.Text.Trim();
                _holiday.HolidayDate = dtpDate.Value.Date;
                _holiday.Year = (int)nudYear.Value;
                _holiday.IsRecurring = chkRecurring.Checked;
                _holiday.Notes = txtNotes.Text.Trim();
                await Program.HolidayRepo.UpdateAsync(_holiday);
                FormHelper.ShowSuccess("Cập nhật ngày lễ thành công!");
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
