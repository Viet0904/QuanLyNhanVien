using QuanLyNhanVien.Helpers;
using Entities = QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Department
{
    public class FrmDepartmentList : Form
    {
        private DataGridView dgv = null!;
        private Button btnAdd = null!, btnEdit = null!, btnDelete = null!, btnRefresh = null!;
        private readonly string _menuCode = "NS_PB";

        public FrmDepartmentList()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadDataAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Quản Lý Phòng Ban";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = Color.FromArgb(30, 30, 46);

            // Toolbar
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(40, 40, 60),
                Padding = new Padding(8, 8, 0, 0), FlowDirection = FlowDirection.LeftToRight
            };

            btnAdd = CreateToolButton("➕ Thêm", "Add", Color.FromArgb(88, 101, 242));
            btnEdit = CreateToolButton("✏️ Sửa", "Edit", Color.FromArgb(87, 163, 75));
            btnDelete = CreateToolButton("🗑️ Xóa", "Delete", Color.FromArgb(200, 60, 60));
            btnRefresh = CreateToolButton("🔄 Làm mới", "", Color.FromArgb(100, 100, 140));

            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });
            this.Controls.Add(toolbar);

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            // DataGridView
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.FromArgb(35, 35, 55),
                GridColor = Color.FromArgb(60, 60, 80),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.FromArgb(200, 210, 230),
                    SelectionBackColor = Color.FromArgb(88, 101, 242), Font = new Font("Segoe UI", 10F)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(50, 50, 75), ForeColor = Color.FromArgb(180, 190, 220),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                },
                EnableHeadersVisualStyles = false, RowHeadersVisible = false, BorderStyle = BorderStyle.None
            };
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };
            this.Controls.Add(dgv);
        }

        private Button CreateToolButton(string text, string tag, Color color)
        {
            var btn = new Button
            {
                Text = text, Tag = tag, Size = new Size(120, 35), FlatStyle = FlatStyle.Flat,
                BackColor = color, ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 8, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private async Task LoadDataAsync()
        {
            var depts = (await Program.DeptService.GetAllAsync(false)).ToList();
            dgv.DataSource = null;
            dgv.DataSource = depts;

            // Ẩn cột không cần
            if (dgv.Columns.Contains("Children")) dgv.Columns["Children"].Visible = false;
            if (dgv.Columns.Contains("DepartmentId")) dgv.Columns["DepartmentId"].Visible = false;
            if (dgv.Columns.Contains("ParentId")) dgv.Columns["ParentId"].Visible = false;
            if (dgv.Columns.Contains("ManagerId")) dgv.Columns["ManagerId"].Visible = false;

            // Header tiếng Việt
            SetColumnHeader("DepartmentCode", "Mã phòng ban");
            SetColumnHeader("DepartmentName", "Tên phòng ban");
            SetColumnHeader("ParentName", "Phòng ban cha");
            SetColumnHeader("ManagerName", "Trưởng phòng");
            SetColumnHeader("EmployeeCount", "Số NV");
            SetColumnHeader("IsActive", "Hoạt động");
        }

        private void SetColumnHeader(string colName, string header)
        {
            if (dgv.Columns.Contains(colName)) dgv.Columns[colName].HeaderText = header;
        }

        private Entities.Department? GetSelectedDept()
        {
            if (dgv.CurrentRow == null) return null;
            return dgv.CurrentRow.DataBoundItem as Entities.Department;
        }

        private async void BtnAdd_Click(object? sender, EventArgs e)
        {
            var dlg = new FrmDepartmentDetail(null);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            var dept = GetSelectedDept();
            if (dept == null) { FormHelper.ShowError("Vui lòng chọn phòng ban."); return; }
            var dlg = new FrmDepartmentDetail(dept);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            var dept = GetSelectedDept();
            if (dept == null) { FormHelper.ShowError("Vui lòng chọn phòng ban."); return; }
            if (!FormHelper.ConfirmDelete(dept.DepartmentName)) return;

            var (ok, msg) = await Program.DeptService.DeleteAsync(dept.DepartmentId);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
            else FormHelper.ShowError(msg);
        }
    }

    public class FrmDepartmentDetail : Form
    {
        private TextBox txtCode = null!, txtName = null!;
        private Entities.Department? _dept;

        public FrmDepartmentDetail(Entities.Department? dept)
        {
            _dept = dept;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = _dept == null ? "Thêm Phòng Ban" : "Sửa Phòng Ban";
            this.Size = new Size(450, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(40, 40, 60);
            this.Font = new Font("Segoe UI", 10F);

            var y = 20;
            AddLabel("Mã phòng ban:", 20, y);
            txtCode = AddTextBox(160, y, 240); y += 45;
            AddLabel("Tên phòng ban:", 20, y);
            txtName = AddTextBox(160, y, 240); y += 55;

            var btnSave = new Button
            {
                Text = "💾 Lưu", Size = new Size(120, 40), Location = new Point(160, y),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(88, 101, 242),
                ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            var btnCancel = new Button
            {
                Text = "Hủy", Size = new Size(80, 40), Location = new Point(290, y),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(80, 80, 100),
                ForeColor = Color.White
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);

            if (_dept != null)
            {
                txtCode.Text = _dept.DepartmentCode;
                txtName.Text = _dept.DepartmentName;
            }
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(x, y + 3), AutoSize = true, ForeColor = Color.FromArgb(180, 190, 220) });
        }

        private TextBox AddTextBox(int x, int y, int w)
        {
            var tb = new TextBox
            {
                Location = new Point(x, y), Size = new Size(w, 30),
                BackColor = Color.FromArgb(55, 55, 80), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(tb);
            return tb;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_dept == null)
            {
                var dept = new Entities.Department { DepartmentCode = txtCode.Text.Trim(), DepartmentName = txtName.Text.Trim() };
                var (ok, msg, _) = await Program.DeptService.CreateAsync(dept);
                if (ok) { this.DialogResult = DialogResult.OK; this.Close(); }
                else FormHelper.ShowError(msg);
            }
            else
            {
                _dept.DepartmentCode = txtCode.Text.Trim();
                _dept.DepartmentName = txtName.Text.Trim();
                var (ok, msg) = await Program.DeptService.UpdateAsync(_dept);
                if (ok) { this.DialogResult = DialogResult.OK; this.Close(); }
                else FormHelper.ShowError(msg);
            }
        }
    }
}
