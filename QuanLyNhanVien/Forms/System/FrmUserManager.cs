using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.SystemAdmin
{
    /// <summary>
    /// Quản lý tài khoản người dùng: CRUD + gán role + reset MK
    /// Theme sáng theo thiết kế Pencil
    /// </summary>
    public class FrmUserManager : Form
    {
        private DataGridView dgv = null!;
        private Button btnAdd = null!, btnEdit = null!, btnDeactivate = null!, btnResetPW = null!;
        private readonly string _menuCode = "HT_USER";

        public FrmUserManager()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadDataAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý người dùng";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;
            this.Padding = new Padding(32, 24, 32, 24);

            // ===== PAGE HEADER =====
            btnDeactivate = ThemeColors.CreateOutlineButton("🚫 Vô hiệu hóa", 150);
            btnDeactivate.Tag = "Delete";
            btnResetPW = ThemeColors.CreateOutlineButton("🔑 Reset mật khẩu", 160);
            btnResetPW.Tag = "Edit";
            btnEdit = ThemeColors.CreateOutlineButton("✏️ Sửa", 100);
            btnEdit.Tag = "Edit";
            btnAdd = ThemeColors.CreatePrimaryButton("+ Thêm mới", 120);
            btnAdd.Tag = "Add";

            var header = ThemeColors.CreatePageHeader(
                "Quản lý người dùng",
                "Quản lý tài khoản và phân quyền hệ thống",
                new Control[] { btnDeactivate, btnResetPW, btnEdit, btnAdd }
            );
            this.Controls.Add(header);

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDeactivate.Click += BtnDeactivate_Click;
            btnResetPW.Click += BtnResetPW_Click;

            // ===== TABLE CONTAINER =====
            var tableContainer = new Panel
            {
                Dock = DockStyle.Fill, BackColor = ThemeColors.Card,
                Padding = new Padding(0)
            };
            tableContainer.Paint += (s, e) =>
            {
                using var pen = new Pen(ThemeColors.Border, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, tableContainer.Width - 1, tableContainer.Height - 1);
            };

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            ThemeColors.StyleDataGridView(dgv);
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };

            tableContainer.Controls.Add(dgv);
            this.Controls.Add(tableContainer);
        }

        private async Task LoadDataAsync()
        {
            var users = (await Program.UserService.GetAllAsync()).ToList();

            if (users.Count == 0) { dgv.DataSource = null; return; }

            var dt = new System.Data.DataTable();
            var first = (IDictionary<string, object>)users[0];
            foreach (var key in first.Keys)
                dt.Columns.Add(key, first[key]?.GetType() ?? typeof(string));
            foreach (var row in users)
            {
                var dict = (IDictionary<string, object>)row;
                dt.Rows.Add(dict.Values.ToArray());
            }

            dgv.DataSource = dt;

            var hideCols = new[] { "PasswordHash", "Salt", "FailedLoginCount", "LockedUntil" };
            foreach (var col in hideCols)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].Visible = false;

            var headers = new Dictionary<string, string>
            {
                ["UserId"] = "ID", ["Username"] = "Tên đăng nhập", ["RoleNames"] = "Vai trò",
                ["IsActive"] = "Kích hoạt", ["LastLogin"] = "Đăng nhập cuối", ["CreatedAt"] = "Ngày tạo"
            };
            foreach (var (col, h) in headers)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].HeaderText = h;

            if (dgv.Columns.Contains("UserId")) dgv.Columns["UserId"].Width = 50;
            if (dgv.Columns.Contains("LastLogin")) dgv.Columns["LastLogin"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            if (dgv.Columns.Contains("CreatedAt")) dgv.Columns["CreatedAt"].DefaultCellStyle.Format = "dd/MM/yyyy";
        }

        private int GetSelectedUserId()
        {
            if (dgv.CurrentRow == null) return 0;
            return Convert.ToInt32(dgv.CurrentRow.Cells["UserId"].Value);
        }

        private string GetSelectedUsername()
        {
            if (dgv.CurrentRow == null) return "";
            return dgv.CurrentRow.Cells["Username"].Value?.ToString() ?? "";
        }

        private async void BtnAdd_Click(object? s, EventArgs e)
        {
            var dlg = new FrmUserDetail(0);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnEdit_Click(object? s, EventArgs e)
        {
            var userId = GetSelectedUserId();
            if (userId == 0) { FormHelper.ShowError("Vui lòng chọn tài khoản."); return; }
            var dlg = new FrmUserDetail(userId);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnDeactivate_Click(object? s, EventArgs e)
        {
            var userId = GetSelectedUserId();
            var username = GetSelectedUsername();
            if (userId == 0) { FormHelper.ShowError("Vui lòng chọn tài khoản."); return; }

            if (MessageBox.Show($"Vô hiệu hóa tài khoản '{username}'?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            var (ok, msg) = await Program.UserService.DeactivateAsync(userId);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
            else FormHelper.ShowError(msg);
        }

        private async void BtnResetPW_Click(object? s, EventArgs e)
        {
            var userId = GetSelectedUserId();
            var username = GetSelectedUsername();
            if (userId == 0) { FormHelper.ShowError("Vui lòng chọn tài khoản."); return; }

            if (MessageBox.Show($"Reset mật khẩu của '{username}' về '123456'?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            var (ok, msg) = await Program.UserService.ResetPasswordAsync(userId);
            if (ok) FormHelper.ShowSuccess(msg);
            else FormHelper.ShowError(msg);
        }
    }

    /// <summary>
    /// Dialog thêm/sửa user (username + roles)
    /// Theme sáng
    /// </summary>
    public class FrmUserDetail : Form
    {
        private TextBox txtUsername = null!, txtPassword = null!;
        private CheckBox chkActive = null!;
        private CheckedListBox clbRoles = null!;
        private readonly int _userId;

        public FrmUserDetail(int userId)
        {
            _userId = userId;
            InitializeComponent();
            this.Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            this.Text = _userId == 0 ? "Thêm tài khoản mới" : "Sửa tài khoản";
            this.Size = new Size(420, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            var y = 20;

            AddLabel("Tên đăng nhập: *", 20, y);
            txtUsername = new TextBox
            {
                Location = new Point(180, y), Size = new Size(200, 30),
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground,
                BorderStyle = BorderStyle.FixedSingle
            };
            if (_userId > 0) txtUsername.ReadOnly = true;
            this.Controls.Add(txtUsername); y += 40;

            if (_userId == 0)
            {
                AddLabel("Mật khẩu: *", 20, y);
                txtPassword = new TextBox
                {
                    Location = new Point(180, y), Size = new Size(200, 30),
                    BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground,
                    BorderStyle = BorderStyle.FixedSingle, PasswordChar = '●'
                };
                this.Controls.Add(txtPassword); y += 40;
            }

            chkActive = new CheckBox
            {
                Text = "Kích hoạt tài khoản", Checked = true,
                Location = new Point(20, y), AutoSize = true,
                ForeColor = ThemeColors.Foreground
            };
            this.Controls.Add(chkActive); y += 35;

            AddLabel("Vai trò:", 20, y); y += 25;
            clbRoles = new CheckedListBox
            {
                Location = new Point(20, y), Size = new Size(360, 130),
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground,
                BorderStyle = BorderStyle.FixedSingle, CheckOnClick = true
            };
            this.Controls.Add(clbRoles); y += 140;

            var btnSave = ThemeColors.CreatePrimaryButton("💾 Lưu", 100, 38);
            btnSave.Location = new Point(100, y);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            var btnCancel = ThemeColors.CreateOutlineButton("Hủy", 80, 38);
            btnCancel.Location = new Point(210, y);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label
            {
                Text = text, Location = new Point(x, y + 3),
                AutoSize = true, ForeColor = ThemeColors.MutedForeground
            });
        }

        private async Task LoadAsync()
        {
            var roles = (await Program.UserService.GetAllRolesAsync()).ToList();
            clbRoles.Items.Clear();
            foreach (var role in roles)
                clbRoles.Items.Add(role, false);
            clbRoles.DisplayMember = "RoleName";

            if (_userId > 0)
            {
                var user = await Program.UserRepo.GetByIdAsync(_userId);
                if (user != null)
                {
                    txtUsername.Text = user.Username;
                    chkActive.Checked = user.IsActive;
                }

                var userRoleIds = (await Program.UserService.GetUserRoleIdsAsync(_userId)).ToList();
                for (int i = 0; i < clbRoles.Items.Count; i++)
                {
                    var role = (Role)clbRoles.Items[i];
                    if (userRoleIds.Contains(role.RoleId))
                        clbRoles.SetItemChecked(i, true);
                }
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            var roleIds = new List<int>();
            foreach (var item in clbRoles.CheckedItems)
            {
                if (item is Role role) roleIds.Add(role.RoleId);
            }

            if (_userId == 0)
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    FormHelper.ShowError("Vui lòng nhập tên đăng nhập.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtPassword?.Text))
                {
                    FormHelper.ShowError("Vui lòng nhập mật khẩu.");
                    return;
                }

                var (ok, msg) = await Program.UserService.CreateAsync(
                    txtUsername.Text.Trim(), txtPassword.Text, roleIds);

                if (ok) { FormHelper.ShowSuccess(msg); this.DialogResult = DialogResult.OK; this.Close(); }
                else FormHelper.ShowError(msg);
            }
            else
            {
                var (ok, msg) = await Program.UserService.UpdateAsync(_userId, chkActive.Checked, roleIds);

                if (ok) { FormHelper.ShowSuccess(msg); this.DialogResult = DialogResult.OK; this.Close(); }
                else FormHelper.ShowError(msg);
            }
        }
    }
}
