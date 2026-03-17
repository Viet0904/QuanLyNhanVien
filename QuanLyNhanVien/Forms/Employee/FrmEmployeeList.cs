using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Employee
{
    public class FrmEmployeeList : Form
    {
        private DataGridView dgv = null!;
        private Button btnAdd = null!, btnEdit = null!, btnDelete = null!, btnRefresh = null!;
        private TextBox txtSearch = null!;
        private ComboBox cboDept = null!;
        private CheckBox chkActive = null!;
        private Label lblCount = null!;
        private readonly string _menuCode = "NS_DSNV";
        private int _page = 1;
        private const int PageSize = 50;

        public FrmEmployeeList()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadFiltersAsync(); await LoadDataAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Danh Sách Nhân Viên";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = Color.FromArgb(30, 30, 46);

            // === Toolbar ===
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(40, 40, 60),
                Padding = new Padding(8, 8, 0, 0)
            };
            btnAdd = CreateBtn("➕ Thêm", "Add", Color.FromArgb(88, 101, 242));
            btnEdit = CreateBtn("✏️ Sửa", "Edit", Color.FromArgb(87, 163, 75));
            btnDelete = CreateBtn("🗑️ Xóa", "Delete", Color.FromArgb(200, 60, 60));
            btnRefresh = CreateBtn("🔄", "", Color.FromArgb(100, 100, 140));
            btnRefresh.Size = new Size(50, 35);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });
            this.Controls.Add(toolbar);

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            // === Filter bar ===
            var filterBar = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(35, 35, 55) };
            this.Controls.Add(filterBar);

            filterBar.Controls.Add(new Label { Text = "🔍", Location = new Point(10, 10), AutoSize = true, ForeColor = Color.White });
            txtSearch = new TextBox
            {
                Location = new Point(35, 8), Size = new Size(200, 30), PlaceholderText = "Tìm theo tên, mã NV...",
                BackColor = Color.FromArgb(50, 50, 75), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) { _page = 1; await LoadDataAsync(); } };
            filterBar.Controls.Add(txtSearch);

            filterBar.Controls.Add(new Label { Text = "Phòng ban:", Location = new Point(250, 12), AutoSize = true, ForeColor = Color.FromArgb(180, 190, 220) });
            cboDept = new ComboBox
            {
                Location = new Point(330, 8), Size = new Size(180, 30), DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 75), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            cboDept.SelectedIndexChanged += async (s, e) => { _page = 1; await LoadDataAsync(); };
            filterBar.Controls.Add(cboDept);

            chkActive = new CheckBox
            {
                Text = "Chỉ NV đang làm", Checked = true, Location = new Point(530, 12),
                AutoSize = true, ForeColor = Color.FromArgb(180, 190, 220)
            };
            chkActive.CheckedChanged += async (s, e) => { _page = 1; await LoadDataAsync(); };
            filterBar.Controls.Add(chkActive);

            lblCount = new Label
            {
                Text = "", Location = new Point(700, 12), AutoSize = true,
                ForeColor = Color.FromArgb(120, 220, 120), Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            filterBar.Controls.Add(lblCount);

            // === DataGridView ===
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.FromArgb(35, 35, 55), GridColor = Color.FromArgb(60, 60, 80),
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

        private Button CreateBtn(string text, string tag, Color c)
        {
            var b = new Button
            {
                Text = text, Tag = tag, Size = new Size(110, 35), FlatStyle = FlatStyle.Flat,
                BackColor = c, ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 8, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private async Task LoadFiltersAsync()
        {
            var depts = (await Program.DeptService.GetAllAsync()).ToList();
            depts.Insert(0, new Models.Entities.Department { DepartmentId = 0, DepartmentName = "-- Tất cả --" });
            cboDept.DataSource = depts;
            cboDept.DisplayMember = "DepartmentName";
            cboDept.ValueMember = "DepartmentId";
        }

        private async Task LoadDataAsync()
        {
            var deptId = cboDept.SelectedValue is int d && d > 0 ? d : (int?)null;
            var result = await Program.EmployeeService.GetListAsync(_page, PageSize, txtSearch.Text.Trim(), deptId, chkActive.Checked ? true : null);

            dgv.DataSource = null;
            dgv.DataSource = result.Items.ToList();

            // Ẩn/đổi tên cột
            var hideCols = new[] { "EmployeeId", "UserId", "Photo", "DepartmentId", "PositionId", "CreatedAt", "UpdatedAt",
                                   "BankAccount", "BankName", "TaxCode", "InsuranceNo", "SalaryCoefficient", "BasicSalary",
                                   "Notes", "TerminationDate", "Address", "IdentityNo" };
            foreach (var col in hideCols)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].Visible = false;

            var headers = new Dictionary<string, string>
            {
                ["EmployeeCode"] = "Mã NV", ["FullName"] = "Họ Tên", ["Gender"] = "Giới Tính",
                ["DateOfBirth"] = "Ngày Sinh", ["Phone"] = "SĐT", ["Email"] = "Email",
                ["DepartmentName"] = "Phòng Ban", ["PositionName"] = "Chức Vụ",
                ["HireDate"] = "Ngày Vào Làm", ["IsActive"] = "Trạng Thái"
            };
            foreach (var (col, header) in headers)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].HeaderText = header;

            lblCount.Text = $"📊 {result.TotalCount} nhân viên";
        }

        private Models.Entities.Employee? GetSelected() => dgv.CurrentRow?.DataBoundItem as Models.Entities.Employee;

        private async void BtnAdd_Click(object? s, EventArgs e)
        {
            var dlg = new FrmEmployeeDetail(null);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnEdit_Click(object? s, EventArgs e)
        {
            var emp = GetSelected();
            if (emp == null) { FormHelper.ShowError("Vui lòng chọn nhân viên."); return; }
            var full = await Program.EmployeeService.GetByIdAsync(emp.EmployeeId);
            var dlg = new FrmEmployeeDetail(full);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadDataAsync();
        }

        private async void BtnDelete_Click(object? s, EventArgs e)
        {
            var emp = GetSelected();
            if (emp == null) { FormHelper.ShowError("Vui lòng chọn nhân viên."); return; }
            if (!FormHelper.ConfirmDelete(emp.FullName)) return;
            var (ok, msg) = await Program.EmployeeService.DeleteAsync(emp.EmployeeId);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
            else FormHelper.ShowError(msg);
        }
    }
}
