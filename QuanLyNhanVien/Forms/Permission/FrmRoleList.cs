using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Permission
{
    /// <summary>
    /// Quản lý vai trò: CRUD roles + gán user vào role
    /// </summary>
    public class FrmRoleList : Form
    {
        private DataGridView dgv = null!;
        private Button btnAdd = null!, btnEdit = null!, btnDelete = null!, btnRefresh = null!;
        private readonly string _menuCode = "HT_VAITRO";

        public FrmRoleList()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Quản Lý Vai Trò";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // Toolbar
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background,
                Padding = new Padding(8, 8, 0, 0)
            };
            btnAdd = CreateBtn("➕ Thêm", ThemeColors.Success); btnAdd.Tag = "Add";
            btnAdd.Click += BtnAdd_Click;
            btnEdit = CreateBtn("✏ Sửa", ThemeColors.Primary); btnEdit.Tag = "Edit";
            btnEdit.Click += BtnEdit_Click;
            btnDelete = CreateBtn("🗑 Xóa", Color.FromArgb(239, 68, 68)); btnDelete.Tag = "Delete";
            btnDelete.Click += async (s, e) => await BtnDelete_Click();
            btnRefresh = CreateBtn("🔄 Làm mới", Color.FromArgb(107, 114, 128));
            btnRefresh.Click += async (s, e) => await LoadAsync();
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });

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
                    SelectionBackColor = ThemeColors.Primary, Font = new Font("Segoe UI", 9.5F)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = ThemeColors.Surface, ForeColor = ThemeColors.MutedForeground,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                },
                EnableHeadersVisualStyles = false, RowHeadersVisible = false, BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 40, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            dgv.CellDoubleClick += (s, e) => BtnEdit_Click(s, e);

            this.Controls.Add(dgv);
            this.Controls.Add(toolbar);
        }

        private Button CreateBtn(string text, Color c)
        {
            var b = new Button
            {
                Text = text, Size = new Size(120, 35), FlatStyle = FlatStyle.Flat,
                BackColor = c, ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 8, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private async Task LoadAsync()
        {
            var roles = (await Program.RoleRepo.GetAllAsync()).ToList();
            dgv.DataSource = null;
            dgv.DataSource = roles;

            if (dgv.Columns.Contains("RoleId")) dgv.Columns["RoleId"].Visible = false;
            if (dgv.Columns.Contains("RoleName")) dgv.Columns["RoleName"].HeaderText = "Tên Vai Trò";
            if (dgv.Columns.Contains("Description")) dgv.Columns["Description"].HeaderText = "Mô Tả";
            if (dgv.Columns.Contains("IsSystem")) { dgv.Columns["IsSystem"].HeaderText = "Hệ Thống"; dgv.Columns["IsSystem"].Width = 80; }
            if (dgv.Columns.Contains("IsActive")) { dgv.Columns["IsActive"].HeaderText = "Kích Hoạt"; dgv.Columns["IsActive"].Width = 80; }
        }

        private void BtnAdd_Click(object? s, EventArgs e)
        {
            using var dlg = new FrmRoleEdit(null);
            if (dlg.ShowDialog() == DialogResult.OK) _ = LoadAsync();
        }

        private void BtnEdit_Click(object? s, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            var role = (Role)dgv.CurrentRow.DataBoundItem;
            using var dlg = new FrmRoleEdit(role);
            if (dlg.ShowDialog() == DialogResult.OK) _ = LoadAsync();
        }

        private async Task BtnDelete_Click()
        {
            if (dgv.CurrentRow == null) return;
            var role = (Role)dgv.CurrentRow.DataBoundItem;
            if (role.IsSystem) { FormHelper.ShowError("Không thể xóa vai trò hệ thống!"); return; }
            if (!FormHelper.ConfirmDelete(role.RoleName)) return;

            // Soft delete by setting IsActive = false
            role.IsActive = false;
            await Program.RoleRepo.UpdateAsync(role);
            FormHelper.ShowSuccess("Đã xóa vai trò!");
            await LoadAsync();
        }
    }

    /// <summary>
    /// Dialog thêm/sửa Role
    /// </summary>
    public class FrmRoleEdit : Form
    {
        private TextBox txtName = null!, txtDesc = null!;
        private Role? _role;

        public FrmRoleEdit(Role? role)
        {
            _role = role;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = _role == null ? "Thêm Vai Trò" : "Sửa Vai Trò";
            this.Size = new Size(420, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            var lbl1 = new Label { Text = "Tên vai trò:", Location = new Point(20, 20), AutoSize = true, ForeColor = ThemeColors.Foreground };
            txtName = new TextBox { Location = new Point(20, 45), Size = new Size(360, 30), BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            if (_role != null) txtName.Text = _role.RoleName;

            var lbl2 = new Label { Text = "Mô tả:", Location = new Point(20, 80), AutoSize = true, ForeColor = ThemeColors.Foreground };
            txtDesc = new TextBox { Location = new Point(20, 105), Size = new Size(360, 30), BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            if (_role != null) txtDesc.Text = _role.Description ?? "";

            var btnSave = new Button
            {
                Text = "💾 Lưu", Location = new Point(140, 155), Size = new Size(120, 38),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Success,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => await SaveAsync();

            this.Controls.AddRange(new Control[] { lbl1, txtName, lbl2, txtDesc, btnSave });
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                FormHelper.ShowError("Tên vai trò không được để trống!");
                return;
            }

            try
            {
                if (_role == null)
                {
                    var newRole = new Role { RoleName = txtName.Text.Trim(), Description = txtDesc.Text.Trim(), IsActive = true };
                    await Program.RoleRepo.InsertAsync(newRole);
                }
                else
                {
                    _role.RoleName = txtName.Text.Trim();
                    _role.Description = txtDesc.Text.Trim();
                    await Program.RoleRepo.UpdateAsync(_role);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                FormHelper.ShowError($"Lỗi lưu: {ex.Message}");
            }
        }
    }
}
