using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Permission
{
    /// <summary>
    /// Form phân quyền: chọn Role → hiển thị menu tree → check 6 quyền per menu
    /// </summary>
    public class FrmRolePermission : Form
    {
        private ComboBox cboRole = null!;
        private TreeView treeMenu = null!;
        private Panel panelPerms = null!;
        private CheckBox chkView = null!, chkAdd = null!, chkEdit = null!, chkDelete = null!, chkExport = null!, chkPrint = null!;
        private Button btnSave = null!;
        private Label lblMenuName = null!;

        private List<MenuNode> _allMenus = new();
        private Dictionary<int, RolePermission> _permissions = new();
        private int? _selectedMenuId;

        public FrmRolePermission()
        {
            InitializeComponent();
            this.Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Phân Quyền Vai Trò";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = Color.FromArgb(30, 30, 46);

            // Top: Role selector
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(40, 40, 60), Padding = new Padding(10) };
            topPanel.Controls.Add(new Label { Text = "Vai trò:", Location = new Point(10, 14), AutoSize = true, ForeColor = Color.FromArgb(180, 190, 220) });
            cboRole = new ComboBox { Location = new Point(80, 10), Size = new Size(250, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(55, 55, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            cboRole.SelectedIndexChanged += async (s, e) => await LoadPermissionsAsync();
            topPanel.Controls.Add(cboRole);

            btnSave = new Button { Text = "💾 Lưu Quyền", Size = new Size(140, 35), Location = new Point(350, 8), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(88, 101, 242), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            topPanel.Controls.Add(btnSave);
            this.Controls.Add(topPanel);

            // Left: Menu tree
            treeMenu = new TreeView
            {
                Dock = DockStyle.Left, Width = 300, BackColor = Color.FromArgb(40, 40, 60),
                ForeColor = Color.FromArgb(200, 210, 230), Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.None, ItemHeight = 32, ShowLines = false, FullRowSelect = true
            };
            treeMenu.AfterSelect += TreeMenu_AfterSelect;
            this.Controls.Add(treeMenu);

            // Right: Permission checkboxes
            panelPerms = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(35, 35, 55), Padding = new Padding(20) };
            this.Controls.Add(panelPerms);

            lblMenuName = new Label
            {
                Text = "Chọn menu bên trái để xem quyền", Dock = DockStyle.Top, Height = 40,
                ForeColor = Color.FromArgb(200, 210, 240), Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelPerms.Controls.Add(lblMenuName);

            var checksPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 60), Size = new Size(400, 250),
                FlowDirection = FlowDirection.TopDown, BackColor = Color.Transparent
            };
            panelPerms.Controls.Add(checksPanel);

            chkView = CreatePermCheck("👁️ Xem (View)"); checksPanel.Controls.Add(chkView);
            chkAdd = CreatePermCheck("➕ Thêm (Add)"); checksPanel.Controls.Add(chkAdd);
            chkEdit = CreatePermCheck("✏️ Sửa (Edit)"); checksPanel.Controls.Add(chkEdit);
            chkDelete = CreatePermCheck("🗑️ Xóa (Delete)"); checksPanel.Controls.Add(chkDelete);
            chkExport = CreatePermCheck("📥 Xuất (Export)"); checksPanel.Controls.Add(chkExport);
            chkPrint = CreatePermCheck("🖨️ In (Print)"); checksPanel.Controls.Add(chkPrint);

            // Check All / Uncheck All
            var btnCheckAll = new Button { Text = "✅ Chọn tất cả", Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 140, 80), ForeColor = Color.White, Margin = new Padding(0, 15, 0, 0) };
            btnCheckAll.FlatAppearance.BorderSize = 0;
            btnCheckAll.Click += (s, e) => SetAllChecks(true);
            checksPanel.Controls.Add(btnCheckAll);

            var btnUncheckAll = new Button { Text = "⬜ Bỏ tất cả", Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(100, 100, 130), ForeColor = Color.White, Margin = new Padding(0, 5, 0, 0) };
            btnUncheckAll.FlatAppearance.BorderSize = 0;
            btnUncheckAll.Click += (s, e) => SetAllChecks(false);
            checksPanel.Controls.Add(btnUncheckAll);
        }

        private CheckBox CreatePermCheck(string text)
        {
            var chk = new CheckBox
            {
                Text = text, AutoSize = true, ForeColor = Color.FromArgb(200, 210, 230),
                Font = new Font("Segoe UI", 11F), Padding = new Padding(5),
                Margin = new Padding(0, 5, 0, 5)
            };
            chk.CheckedChanged += PermCheck_Changed;
            return chk;
        }

        private async Task LoadAsync()
        {
            var roles = (await Program.MenuService.GetAllRolesAsync()).ToList();
            cboRole.DataSource = roles;
            cboRole.DisplayMember = "RoleName";
            cboRole.ValueMember = "RoleId";

            _allMenus = (await Program.MenuService.GetMenuTreeAsync()).ToList();
            BuildMenuTree();
        }

        private void BuildMenuTree()
        {
            treeMenu.Nodes.Clear();
            foreach (var root in _allMenus)
            {
                var node = CreateNode(root);
                treeMenu.Nodes.Add(node);
            }
            treeMenu.ExpandAll();
        }

        private TreeNode CreateNode(MenuNode menu)
        {
            var node = new TreeNode(menu.MenuName) { Tag = menu.MenuId };
            foreach (var child in menu.Children)
                node.Nodes.Add(CreateNode(child));
            return node;
        }

        private async Task LoadPermissionsAsync()
        {
            if (cboRole.SelectedValue is not int roleId) return;
            var perms = (await Program.MenuService.GetRolePermissionsAsync(roleId)).ToList();
            _permissions = perms.ToDictionary(p => p.MenuId);

            // Update checkmarks on tree nodes
            UpdateTreeCheckmarks(treeMenu.Nodes);

            _selectedMenuId = null;
            lblMenuName.Text = "Chọn menu bên trái để xem quyền";
            SetAllChecks(false, false);
        }

        private void UpdateTreeCheckmarks(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is int menuId && _permissions.TryGetValue(menuId, out var perm))
                {
                    node.Text = perm.CanView ? $"✅ {node.Text.TrimStart('✅', '⬜', ' ')}" : $"⬜ {node.Text.TrimStart('✅', '⬜', ' ')}";
                }
                UpdateTreeCheckmarks(node.Nodes);
            }
        }

        private void TreeMenu_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is not int menuId) return;
            _selectedMenuId = menuId;
            lblMenuName.Text = $"Quyền cho: {e.Node.Text.TrimStart('✅', '⬜', ' ')}";

            if (_permissions.TryGetValue(menuId, out var perm))
            {
                chkView.Checked = perm.CanView;
                chkAdd.Checked = perm.CanAdd;
                chkEdit.Checked = perm.CanEdit;
                chkDelete.Checked = perm.CanDelete;
                chkExport.Checked = perm.CanExport;
                chkPrint.Checked = perm.CanPrint;
            }
            else
            {
                SetAllChecks(false, false);
            }
        }

        private void PermCheck_Changed(object? sender, EventArgs e)
        {
            if (_selectedMenuId == null || cboRole.SelectedValue is not int roleId) return;
            var menuId = _selectedMenuId.Value;

            if (!_permissions.ContainsKey(menuId))
                _permissions[menuId] = new RolePermission { RoleId = roleId, MenuId = menuId };

            _permissions[menuId].CanView = chkView.Checked;
            _permissions[menuId].CanAdd = chkAdd.Checked;
            _permissions[menuId].CanEdit = chkEdit.Checked;
            _permissions[menuId].CanDelete = chkDelete.Checked;
            _permissions[menuId].CanExport = chkExport.Checked;
            _permissions[menuId].CanPrint = chkPrint.Checked;
        }

        private void SetAllChecks(bool value, bool updatePerm = true)
        {
            chkView.Checked = value; chkAdd.Checked = value; chkEdit.Checked = value;
            chkDelete.Checked = value; chkExport.Checked = value; chkPrint.Checked = value;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cboRole.SelectedValue is not int roleId) return;
            try
            {
                await Program.MenuService.SaveRolePermissionsAsync(roleId, _permissions.Values.ToList());
                FormHelper.ShowSuccess("Lưu phân quyền thành công!");
            }
            catch (Exception ex)
            {
                FormHelper.ShowError($"Lỗi lưu quyền: {ex.Message}");
            }
        }
    }
}
