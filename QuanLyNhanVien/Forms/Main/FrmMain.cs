using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.DTOs;
using QuanLyNhanVien.Models.Enums;
using QuanLyNhanVien.Forms.Employee;
using QuanLyNhanVien.Forms.Department;
using QuanLyNhanVien.Forms.Position;
using QuanLyNhanVien.Forms.Permission;
using QuanLyNhanVien.Forms.Category;

namespace QuanLyNhanVien.Forms.Main
{
    public class FrmMain : Form
    {
        private Panel panelMenu = null!;
        private Panel panelContent = null!;
        private Panel panelHeader = null!;
        private Panel panelStatus = null!;
        private TreeView treeMenu = null!;
        private Label lblCurrentUser = null!;
        private Label lblStatusInfo = null!;
        private readonly Dictionary<string, string> _menuFormMap = new();

        public FrmMain()
        {
            InitializeComponent();
            this.Load += FrmMain_Load;
        }

        private void InitializeComponent()
        {
            this.Text = $"Quản Lý Nhân Viên v1.0 - {AppSession.DisplayName}";
            this.Size = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(30, 30, 46);
            this.Font = new Font("Segoe UI", 10F);

            // ===== HEADER =====
            panelHeader = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.FromArgb(35, 35, 55) };
            panelHeader.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(88, 101, 242), 2);
                e.Graphics.DrawLine(pen, 0, panelHeader.Height - 1, panelHeader.Width, panelHeader.Height - 1);
            };
            this.Controls.Add(panelHeader);

            var lblAppTitle = new Label
            {
                Text = "📋 QUẢN LÝ NHÂN VIÊN",
                ForeColor = Color.FromArgb(200, 210, 255),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Location = new Point(15, 10), AutoSize = true
            };
            panelHeader.Controls.Add(lblAppTitle);

            lblCurrentUser = new Label
            {
                Text = $"👤 {AppSession.DisplayName} | {AppSession.RoleDisplay}",
                ForeColor = Color.FromArgb(160, 170, 200),
                Font = new Font("Segoe UI", 10F),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = true, Location = new Point(panelHeader.Width - 300, 18)
            };
            panelHeader.Controls.Add(lblCurrentUser);

            var btnLogout = new Button
            {
                Text = "🚪 Đăng xuất", Size = new Size(120, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(200, 60, 60),
                ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                { AppSession.Clear(); this.Close(); }
            };
            panelHeader.Controls.Add(btnLogout);
            panelHeader.Resize += (s, e) =>
            {
                lblCurrentUser.Location = new Point(panelHeader.Width - lblCurrentUser.Width - 140, 18);
                btnLogout.Location = new Point(panelHeader.Width - 130, 10);
            };

            // ===== STATUS BAR =====
            panelStatus = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(35, 35, 55) };
            this.Controls.Add(panelStatus);
            lblStatusInfo = new Label
            {
                Text = $"  Sẵn sàng | {DateTime.Now:dd/MM/yyyy HH:mm}",
                ForeColor = Color.FromArgb(140, 150, 180), Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9F)
            };
            panelStatus.Controls.Add(lblStatusInfo);
            var timer = new System.Windows.Forms.Timer { Interval = 60000 };
            timer.Tick += (s, e) => { lblStatusInfo.Text = $"  {AppSession.DisplayName} | {AppSession.RoleDisplay} | {DateTime.Now:dd/MM/yyyy HH:mm}"; };
            timer.Start();

            // ===== MENU PANEL =====
            panelMenu = new Panel { Dock = DockStyle.Left, Width = 260, BackColor = Color.FromArgb(40, 40, 60), Padding = new Padding(5) };
            panelMenu.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(60, 60, 90), 1);
                e.Graphics.DrawLine(pen, panelMenu.Width - 1, 0, panelMenu.Width - 1, panelMenu.Height);
            };
            this.Controls.Add(panelMenu);

            treeMenu = new TreeView
            {
                Dock = DockStyle.Fill, BackColor = Color.FromArgb(40, 40, 60),
                ForeColor = Color.FromArgb(200, 210, 230), Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.None, ItemHeight = 36, ShowLines = false,
                ShowRootLines = false, FullRowSelect = true, HideSelection = false, Indent = 25
            };
            treeMenu.NodeMouseClick += TreeMenu_NodeClick;
            panelMenu.Controls.Add(treeMenu);

            // ===== CONTENT PANEL =====
            panelContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 46), Padding = new Padding(0) };
            this.Controls.Add(panelContent);
            panelContent.Controls.Add(new Label
            {
                Text = $"Chào mừng, {AppSession.DisplayName}!\n\nChọn một mục trong menu bên trái để bắt đầu.",
                ForeColor = Color.FromArgb(150, 160, 190), Font = new Font("Segoe UI", 14F),
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter
            });
        }

        private void FrmMain_Load(object? sender, EventArgs e) => BuildMenuTree();

        private void BuildMenuTree()
        {
            treeMenu.Nodes.Clear();
            _menuFormMap.Clear();
            var tree = Program.MenuService.BuildPermissionTree(AppSession.Permissions);
            foreach (var root in tree)
            {
                var node = CreateTreeNode(root);
                if (node != null) treeMenu.Nodes.Add(node);
            }
            treeMenu.ExpandAll();
        }

        private TreeNode? CreateTreeNode(MenuPermissionDto menu)
        {
            if (!menu.CanView && !menu.Children.Any(c => c.CanView)) return null;
            var icon = GetMenuIcon(menu.IconName);
            var node = new TreeNode($"{icon} {menu.MenuName}") { Tag = menu.MenuCode };
            if (!string.IsNullOrEmpty(menu.FormName)) _menuFormMap[menu.MenuCode] = menu.FormName;
            foreach (var child in menu.Children.Where(c => c.CanView))
            {
                var cn = CreateTreeNode(child);
                if (cn != null) node.Nodes.Add(cn);
            }
            return node;
        }

        private string GetMenuIcon(string? iconName) => iconName switch
        {
            "people" => "👥", "user" => "👤", "organization" => "🏢", "badge" => "🎖️",
            "clock" => "⏰", "calendar-check" => "📅", "calendar" => "🗓️", "document" => "📄",
            "money" => "💰", "calculator" => "🧮", "receipt" => "🧾", "history" => "📜",
            "settings" => "⚙️", "chart" => "📊", "dashboard" => "📈", "report" => "📋",
            "shield" => "🛡️", "key" => "🔑", "menu" => "☰", "list" => "📝", "log" => "📒",
            _ => "▸"
        };

        private void TreeMenu_NodeClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            var menuCode = e.Node.Tag?.ToString();
            if (string.IsNullOrEmpty(menuCode) || !_menuFormMap.ContainsKey(menuCode)) return;
            OpenChildForm(_menuFormMap[menuCode], menuCode);
        }

        private void OpenChildForm(string formName, string menuCode)
        {
            foreach (Control c in panelContent.Controls)
            {
                if (c is Form f) f.Close();
                c.Dispose();
            }
            panelContent.Controls.Clear();

            Form? childForm = formName switch
            {
                "FrmEmployeeList" => new FrmEmployeeList(),
                "FrmDepartmentList" => new FrmDepartmentList(),
                "FrmPositionList" => new FrmPositionList(),
                "FrmRolePermission" => new FrmRolePermission(),
                "FrmMenuManager" => new FrmMenuManager(),
                "FrmCategoryManager" => new FrmCategoryManager(),
                _ => null
            };

            if (childForm != null)
            {
                FormHelper.EmbedForm(panelContent, childForm);
            }
            else
            {
                panelContent.Controls.Add(new Label
                {
                    Text = $"🚧 {formName}\n\nForm này sẽ được triển khai ở các phase tiếp theo.\n\nMenu Code: {menuCode}",
                    ForeColor = Color.FromArgb(150, 160, 190), Font = new Font("Segoe UI", 14F),
                    Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter
                });
            }
            lblStatusInfo.Text = $"  📂 {formName} | {AppSession.DisplayName} | {DateTime.Now:dd/MM/yyyy HH:mm}";
        }
    }
}
