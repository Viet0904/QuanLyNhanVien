using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Permission
{
    /// <summary>
    /// Quản lý menu: thêm/sửa/xóa menu items, sắp xếp thứ tự
    /// </summary>
    public class FrmMenuManager : Form
    {
        private TreeView treeMenu = null!;
        private TextBox txtCode = null!, txtName = null!, txtFormName = null!, txtIcon = null!;
        private NumericUpDown nudOrder = null!;
        private ComboBox cboParent = null!;
        private Button btnSave = null!, btnNew = null!, btnDelete = null!;
        private MenuNode? _selectedMenu;
        private List<MenuNode> _flatMenus = new();

        public FrmMenuManager()
        {
            InitializeComponent();
            this.Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản Lý Menu";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // Left: Menu tree
            var leftPanel = new Panel { Dock = DockStyle.Left, Width = 300, BackColor = ThemeColors.Background };

            var btnToolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(45, 45, 65), Padding = new Padding(5) };
            btnNew = new Button { Text = "➕ Mới", Size = new Size(80, 32), FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Primary, ForeColor = Color.White };
            btnNew.FlatAppearance.BorderSize = 0;
            btnNew.Click += (s, e) => NewMenu();
            btnDelete = new Button { Text = "🗑️ Xóa", Size = new Size(80, 32), FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Error, ForeColor = Color.White, Margin = new Padding(5, 0, 0, 0) };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;
            btnToolbar.Controls.AddRange(new Control[] { btnNew, btnDelete });

            treeMenu = new TreeView
            {
                Dock = DockStyle.Fill, BackColor = ThemeColors.Background,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.None, ItemHeight = 30, ShowLines = true, FullRowSelect = true
            };
            treeMenu.AfterSelect += TreeMenu_AfterSelect;

            // WinForms dock order: Fill added first, Top added last
            leftPanel.Controls.Add(treeMenu);     // Fill — added first
            leftPanel.Controls.Add(btnToolbar);   // Top — added last → docks on top

            // Right: Detail form
            var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background, Padding = new Padding(20) };

            var lblHeader = new Label
            {
                Text = "📝 THÔNG TIN MENU", Dock = DockStyle.Top, Height = 40,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeColors.Foreground, TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            var formPanel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeColors.Background };
            var y = 20;
            AddLabel(formPanel, "Mã menu:", 20, y);
            txtCode = AddTxt(formPanel, 150, y, 200); y += 40;
            AddLabel(formPanel, "Tên menu:", 20, y);
            txtName = AddTxt(formPanel, 150, y, 250); y += 40;
            AddLabel(formPanel, "Tên Form:", 20, y);
            txtFormName = AddTxt(formPanel, 150, y, 250); y += 40;
            AddLabel(formPanel, "Icon:", 20, y);
            txtIcon = AddTxt(formPanel, 150, y, 150); y += 40;
            AddLabel(formPanel, "Menu cha:", 20, y);
            cboParent = new ComboBox { Location = new Point(150, y), Size = new Size(250, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat };
            formPanel.Controls.Add(cboParent); y += 40;
            AddLabel(formPanel, "Thứ tự:", 20, y);
            nudOrder = new NumericUpDown { Location = new Point(150, y), Size = new Size(80, 30), Minimum = 0, Maximum = 100, BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground };
            formPanel.Controls.Add(nudOrder); y += 55;

            btnSave = new Button { Text = "💾 Lưu", Size = new Size(130, 40), Location = new Point(150, y), FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Primary, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            formPanel.Controls.Add(btnSave);

            // WinForms dock: Fill first, Top last
            rightPanel.Controls.Add(formPanel);   // Fill — added first
            rightPanel.Controls.Add(lblHeader);    // Top — added last → docks on top

            // WinForms dock order: Fill first, then Left
            this.Controls.Add(rightPanel);  // Fill — added first
            this.Controls.Add(leftPanel);   // Left — added after → docks before Fill
        }

        private void AddLabel(Control p, string t, int x, int y) { p.Controls.Add(new Label { Text = t, Location = new Point(x, y + 3), AutoSize = true, ForeColor = ThemeColors.MutedForeground }); }
        private TextBox AddTxt(Control p, int x, int y, int w)
        {
            var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 30), BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(tb); return tb;
        }

        private async Task LoadAsync()
        {
            var tree = await Program.MenuService.GetMenuTreeAsync();
            _flatMenus = FlattenTree(tree);

            // Tree
            treeMenu.Nodes.Clear();
            foreach (var root in tree) treeMenu.Nodes.Add(BuildNode(root));
            treeMenu.ExpandAll();

            // Parent combo
            var parents = new List<MenuNode> { new MenuNode { MenuId = 0, MenuName = "(Gốc - Không có cha)" } };
            parents.AddRange(_flatMenus);
            cboParent.DataSource = parents;
            cboParent.DisplayMember = "MenuName";
            cboParent.ValueMember = "MenuId";
        }

        private TreeNode BuildNode(MenuNode m)
        {
            var n = new TreeNode(m.MenuName) { Tag = m.MenuId };
            foreach (var c in m.Children) n.Nodes.Add(BuildNode(c));
            return n;
        }

        private List<MenuNode> FlattenTree(List<MenuNode> nodes)
        {
            var list = new List<MenuNode>();
            foreach (var n in nodes) { list.Add(n); list.AddRange(FlattenTree(n.Children)); }
            return list;
        }

        private void TreeMenu_AfterSelect(object? s, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is not int menuId) return;
            _selectedMenu = _flatMenus.FirstOrDefault(m => m.MenuId == menuId);
            if (_selectedMenu == null) return;

            txtCode.Text = _selectedMenu.MenuCode;
            txtName.Text = _selectedMenu.MenuName;
            txtFormName.Text = _selectedMenu.FormName;
            txtIcon.Text = _selectedMenu.IconName;
            nudOrder.Value = _selectedMenu.SortOrder;
            cboParent.SelectedValue = _selectedMenu.ParentId ?? 0;
        }

        private void NewMenu()
        {
            _selectedMenu = null;
            txtCode.Clear(); txtName.Clear(); txtFormName.Clear(); txtIcon.Clear();
            nudOrder.Value = 0; cboParent.SelectedIndex = 0;
            txtCode.Focus();
        }

        private async void BtnSave_Click(object? s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                FormHelper.ShowError("Vui lòng nhập mã menu và tên menu.");
                return;
            }

            var parentId = cboParent.SelectedValue is int p && p > 0 ? p : (int?)null;
            if (_selectedMenu == null)
            {
                var menu = new MenuNode
                {
                    MenuCode = txtCode.Text.Trim(), MenuName = txtName.Text.Trim(),
                    FormName = txtFormName.Text.Trim(), IconName = txtIcon.Text.Trim(),
                    ParentId = parentId, SortOrder = (int)nudOrder.Value
                };
                await Program.MenuService.CreateMenuAsync(menu);
                FormHelper.ShowSuccess("Thêm menu thành công!");
            }
            else
            {
                _selectedMenu.MenuCode = txtCode.Text.Trim();
                _selectedMenu.MenuName = txtName.Text.Trim();
                _selectedMenu.FormName = txtFormName.Text.Trim();
                _selectedMenu.IconName = txtIcon.Text.Trim();
                _selectedMenu.ParentId = parentId;
                _selectedMenu.SortOrder = (int)nudOrder.Value;
                await Program.MenuService.UpdateMenuAsync(_selectedMenu);
                FormHelper.ShowSuccess("Cập nhật menu thành công!");
            }
            await LoadAsync();
        }

        private async void BtnDelete_Click(object? s, EventArgs e)
        {
            if (_selectedMenu == null) return;
            if (!FormHelper.ConfirmDelete(_selectedMenu.MenuName)) return;
            try
            {
                await Program.MenuService.DeleteMenuAsync(_selectedMenu.MenuId);
                FormHelper.ShowSuccess("Xóa menu thành công!");
                await LoadAsync();
                NewMenu();
            }
            catch (Exception ex) { FormHelper.ShowError($"Lỗi: {ex.Message}"); }
        }
    }
}
