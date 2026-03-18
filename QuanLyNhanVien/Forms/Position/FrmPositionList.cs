using QuanLyNhanVien.Helpers;
using Entities = QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Position
{
    public class FrmPositionList : Form
    {
        private DataGridView dgv = null!;
        private Button btnAdd = null!, btnEdit = null!, btnDelete = null!, btnRefresh = null!;
        private readonly string _menuCode = "NS_CV";

        public FrmPositionList()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadDataAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Quản Lý Chức Vụ";
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
            btnDelete = CreateBtn("🗑️ Xóa", "Delete", ThemeColors.Error);
            btnRefresh = CreateBtn("🔄 Làm mới", "", ThemeColors.MutedForeground);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });


            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
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
                EnableHeadersVisualStyles = false, RowHeadersVisible = false, BorderStyle = BorderStyle.None
            };
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };
            // WinForms dock order: Fill first, Top after
            this.Controls.Add(dgv);       // Fill
            this.Controls.Add(toolbar);   // Top
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
            var list = (await Program.PosService.GetAllAsync(false)).ToList();
            dgv.DataSource = null;
            dgv.DataSource = list;
            if (dgv.Columns.Contains("PositionId")) dgv.Columns["PositionId"].Visible = false;
            if (dgv.Columns.Contains("PositionName")) dgv.Columns["PositionName"].HeaderText = "Tên Chức Vụ";
            if (dgv.Columns.Contains("Level")) dgv.Columns["Level"].HeaderText = "Cấp bậc";
            if (dgv.Columns.Contains("AllowanceAmount")) { dgv.Columns["AllowanceAmount"].HeaderText = "Phụ cấp"; dgv.Columns["AllowanceAmount"].DefaultCellStyle.Format = "N0"; }
            if (dgv.Columns.Contains("IsActive")) dgv.Columns["IsActive"].HeaderText = "Hoạt động";
        }

        private Entities.Position? GetSelected() => dgv.CurrentRow?.DataBoundItem as Entities.Position;

        private async void BtnAdd_Click(object? s, EventArgs e)
        {
            var dlg = new FrmPositionDetail(null);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnEdit_Click(object? s, EventArgs e)
        {
            var pos = GetSelected();
            if (pos == null) return;
            var dlg = new FrmPositionDetail(pos);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnDelete_Click(object? s, EventArgs e)
        {
            var pos = GetSelected();
            if (pos == null) return;
            if (!FormHelper.ConfirmDelete(pos.PositionName)) return;
            var (ok, msg) = await Program.PosService.DeleteAsync(pos.PositionId);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
            else FormHelper.ShowError(msg);
        }
    }

    public class FrmPositionDetail : Form
    {
        private TextBox txtName = null!;
        private NumericUpDown nudLevel = null!, nudAllowance = null!;
        private Entities.Position? _pos;

        public FrmPositionDetail(Entities.Position? pos)
        {
            _pos = pos;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = _pos == null ? "Thêm Chức Vụ" : "Sửa Chức Vụ";
            this.Size = new Size(420, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            var y = 20;
            AddLbl("Tên chức vụ:", 20, y);
            txtName = new TextBox { Location = new Point(150, y), Size = new Size(230, 30), BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(txtName); y += 45;

            AddLbl("Cấp bậc:", 20, y);
            nudLevel = new NumericUpDown { Location = new Point(150, y), Size = new Size(100, 30), Minimum = 0, Maximum = 20, BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground };
            this.Controls.Add(nudLevel); y += 45;

            AddLbl("Phụ cấp (VNĐ):", 20, y);
            nudAllowance = new NumericUpDown { Location = new Point(150, y), Size = new Size(180, 30), Minimum = 0, Maximum = 100000000, Increment = 500000, ThousandsSeparator = true, BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground };
            this.Controls.Add(nudAllowance); y += 55;

            var btnSave = new Button { Text = "💾 Lưu", Size = new Size(110, 40), Location = new Point(150, y), FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Primary, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            var btnCancel = new Button { Text = "Hủy", Size = new Size(80, 40), Location = new Point(270, y), FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.MutedForeground, ForeColor = ThemeColors.Foreground };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);

            if (_pos != null)
            {
                txtName.Text = _pos.PositionName;
                nudLevel.Value = _pos.Level;
                nudAllowance.Value = _pos.AllowanceAmount;
            }
        }

        private void AddLbl(string text, int x, int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(x, y + 3), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
        }

        private async void BtnSave_Click(object? s, EventArgs e)
        {
            if (_pos == null)
            {
                var pos = new Entities.Position { PositionName = txtName.Text.Trim(), Level = (int)nudLevel.Value, AllowanceAmount = nudAllowance.Value };
                var (ok, msg, _) = await Program.PosService.CreateAsync(pos);
                if (ok) { this.DialogResult = DialogResult.OK; this.Close(); } else FormHelper.ShowError(msg);
            }
            else
            {
                _pos.PositionName = txtName.Text.Trim();
                _pos.Level = (int)nudLevel.Value;
                _pos.AllowanceAmount = nudAllowance.Value;
                var (ok, msg) = await Program.PosService.UpdateAsync(_pos);
                if (ok) { this.DialogResult = DialogResult.OK; this.Close(); } else FormHelper.ShowError(msg);
            }
        }
    }
}
