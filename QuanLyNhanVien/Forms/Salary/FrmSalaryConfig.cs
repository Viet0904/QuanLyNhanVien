using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Salary
{
    /// <summary>
    /// Cấu hình lương: BHXH, BHYT, giảm trừ, ngày công chuẩn...
    /// </summary>
    public class FrmSalaryConfig : Form
    {
        private DataGridView dgv = null!;
        private Button btnAdd = null!, btnEdit = null!, btnRefresh = null!;
        private readonly string _menuCode = "LG_CAUHINH";

        public FrmSalaryConfig()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadDataAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Cấu Hình Lương";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // Toolbar
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background,
                Padding = new Padding(8, 8, 0, 0)
            };
            btnAdd = CreateBtn("➕ Thêm", "Add", ThemeColors.Primary);
            btnEdit = CreateBtn("✏️ Sửa", "Edit", ThemeColors.Success);
            btnRefresh = CreateBtn("🔄 Làm mới", "", ThemeColors.MutedForeground);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnRefresh });

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

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
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };

            // WinForms dock order: Fill first, Top after
            this.Controls.Add(dgv);
            this.Controls.Add(toolbar);
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
            var list = (await Program.SalaryService.GetAllConfigsAsync()).ToList();
            dgv.DataSource = null;
            dgv.DataSource = list;

            if (dgv.Columns.Contains("ConfigId")) dgv.Columns["ConfigId"].Visible = false;
            if (dgv.Columns.Contains("IsActive")) dgv.Columns["IsActive"].Visible = false;
            if (dgv.Columns.Contains("ConfigCode")) dgv.Columns["ConfigCode"].HeaderText = "Mã";
            if (dgv.Columns.Contains("ConfigName")) dgv.Columns["ConfigName"].HeaderText = "Tên cấu hình";
            if (dgv.Columns.Contains("ConfigValue"))
            {
                dgv.Columns["ConfigValue"].HeaderText = "Giá trị";
                dgv.Columns["ConfigValue"].DefaultCellStyle.Format = "N2";
            }
            if (dgv.Columns.Contains("ConfigType")) dgv.Columns["ConfigType"].HeaderText = "Loại";
            if (dgv.Columns.Contains("EffectiveFrom"))
            {
                dgv.Columns["EffectiveFrom"].HeaderText = "Từ ngày";
                dgv.Columns["EffectiveFrom"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            if (dgv.Columns.Contains("EffectiveTo"))
            {
                dgv.Columns["EffectiveTo"].HeaderText = "Đến ngày";
                dgv.Columns["EffectiveTo"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
        }

        private SalaryConfig? GetSelected() => dgv.CurrentRow?.DataBoundItem as SalaryConfig;

        private async void BtnAdd_Click(object? s, EventArgs e)
        {
            var dlg = new FrmSalaryConfigDetail(null);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnEdit_Click(object? s, EventArgs e)
        {
            var cfg = GetSelected();
            if (cfg == null) return;
            var dlg = new FrmSalaryConfigDetail(cfg);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }
    }

    public class FrmSalaryConfigDetail : Form
    {
        private TextBox txtCode = null!, txtName = null!;
        private NumericUpDown nudValue = null!;
        private ComboBox cboType = null!;
        private DateTimePicker dtpFrom = null!;
        private SalaryConfig? _cfg;

        public FrmSalaryConfigDetail(SalaryConfig? cfg)
        {
            _cfg = cfg;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = _cfg == null ? "Thêm Cấu Hình" : "Sửa Cấu Hình";
            this.Size = new Size(450, 340);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            var y = 20;
            AddLbl("Mã:", 20, y);
            txtCode = AddTxt(160, y, 240); y += 40;
            AddLbl("Tên:", 20, y);
            txtName = AddTxt(160, y, 240); y += 40;
            AddLbl("Giá trị:", 20, y);
            nudValue = new NumericUpDown
            {
                Location = new Point(160, y), Size = new Size(200, 30), DecimalPlaces = 2,
                Maximum = 9999999999, BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground
            };
            this.Controls.Add(nudValue); y += 40;
            AddLbl("Loại:", 20, y);
            cboType = new ComboBox
            {
                Location = new Point(160, y), Size = new Size(150, 30), DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground
            };
            cboType.Items.AddRange(new[] { "Percent", "Amount", "Days", "Multiplier" });
            cboType.SelectedIndex = 0;
            this.Controls.Add(cboType); y += 40;
            AddLbl("Hiệu lực từ:", 20, y);
            dtpFrom = new DateTimePicker
            {
                Location = new Point(160, y), Size = new Size(200, 30), Format = DateTimePickerFormat.Short,
                CalendarMonthBackground = ThemeColors.Background
            };
            this.Controls.Add(dtpFrom); y += 50;

            var btnSave = new Button
            {
                Text = "💾 Lưu", Size = new Size(110, 40), Location = new Point(160, y),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Primary,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            var btnCancel = new Button
            {
                Text = "Hủy", Size = new Size(80, 40), Location = new Point(280, y),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.MutedForeground, ForeColor = ThemeColors.Foreground
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);

            if (_cfg != null)
            {
                txtCode.Text = _cfg.ConfigCode;
                txtCode.ReadOnly = true;
                txtName.Text = _cfg.ConfigName;
                nudValue.Value = _cfg.ConfigValue;
                cboType.SelectedItem = _cfg.ConfigType;
                dtpFrom.Value = _cfg.EffectiveFrom;
            }
        }

        private void AddLbl(string text, int x, int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(x, y + 2), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
        }

        private TextBox AddTxt(int x, int y, int w)
        {
            var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 30), BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(tb);
            return tb;
        }

        private async void BtnSave_Click(object? s, EventArgs e)
        {
            var cfg = _cfg ?? new SalaryConfig();
            cfg.ConfigCode = txtCode.Text.Trim();
            cfg.ConfigName = txtName.Text.Trim();
            cfg.ConfigValue = nudValue.Value;
            cfg.ConfigType = cboType.SelectedItem?.ToString() ?? "Percent";
            cfg.EffectiveFrom = dtpFrom.Value.Date;

            var (ok, msg) = await Program.SalaryService.SaveConfigAsync(cfg);
            if (ok) { this.DialogResult = DialogResult.OK; this.Close(); }
            else FormHelper.ShowError(msg);
        }
    }
}
