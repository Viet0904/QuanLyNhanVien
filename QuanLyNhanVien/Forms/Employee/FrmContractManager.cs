using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Employee
{
    /// <summary>
    /// Quản lý hợp đồng lao động
    /// </summary>
    public class FrmContractManager : Form
    {
        private DataGridView dgv = null!;
        private Button btnAdd = null!, btnEdit = null!, btnDelete = null!, btnRefresh = null!;
        private Label lblAlert = null!;
        private readonly string _menuCode = "NS_HOPDONG";

        public FrmContractManager()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadDataAsync(); await CheckExpiringAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Quản Lý Hợp Đồng Lao Động";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = ThemeColors.Background };
            header.Controls.Add(new Label
            {
                Text = "📄 QUẢN LÝ HỢP ĐỒNG LAO ĐỘNG", Dock = DockStyle.Left, AutoSize = false,
                Width = 500, TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Padding = new Padding(15, 0, 0, 0)
            });

            // Toolbar
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background,
                Padding = new Padding(8, 8, 0, 0)
            };
            btnAdd = CreateBtn("➕ Thêm", "Add", ThemeColors.Primary);
            btnEdit = CreateBtn("✏️ Sửa", "Edit", ThemeColors.Success);
            btnDelete = CreateBtn("🗑️ Hủy HĐ", "Delete", ThemeColors.Error);
            btnRefresh = CreateBtn("🔄", "", ThemeColors.MutedForeground);
            btnRefresh.Size = new Size(50, 35);
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            // Alert panel
            lblAlert = new Label
            {
                Dock = DockStyle.Top, Height = 0, BackColor = Color.FromArgb(180, 100, 20),
                ForeColor = ThemeColors.Foreground, TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold), Padding = new Padding(15, 0, 0, 0)
            };

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

            this.Controls.Add(dgv);
            this.Controls.Add(lblAlert);
            this.Controls.Add(toolbar);
            this.Controls.Add(header);
        }

        private Button CreateBtn(string text, string tag, Color c)
        {
            var b = new Button
            {
                Text = text, Tag = tag, Size = new Size(110, 35), FlatStyle = FlatStyle.Flat,
                BackColor = c, ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 8, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private async Task LoadDataAsync()
        {
            var contracts = (await Program.ContractService.GetAllAsync()).ToList();
            dgv.DataSource = contracts;

            var hideCols = new[] { "ContractId", "EmployeeId", "Notes", "CreatedAt" };
            foreach (var c in hideCols)
                if (dgv.Columns.Contains(c)) dgv.Columns[c].Visible = false;

            var headers = new Dictionary<string, string>
            {
                ["ContractCode"] = "Mã HĐ", ["EmployeeCode"] = "Mã NV", ["EmployeeName"] = "Họ Tên",
                ["ContractTypeName"] = "Loại HĐ", ["ContractType"] = "Mã Loại",
                ["SignDate"] = "Ngày Ký", ["StartDate"] = "Bắt Đầu", ["EndDate"] = "Kết Thúc",
                ["IsActive"] = "Hiệu Lực"
            };
            foreach (var (col, h) in headers)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].HeaderText = h;

            if (dgv.Columns.Contains("ContractType")) dgv.Columns["ContractType"].Visible = false;
            foreach (var dateCol in new[] { "SignDate", "StartDate", "EndDate" })
                if (dgv.Columns.Contains(dateCol)) dgv.Columns[dateCol].DefaultCellStyle.Format = "dd/MM/yyyy";
        }

        private async Task CheckExpiringAsync()
        {
            var expiring = (await Program.ContractService.GetExpiringAsync(30)).ToList();
            if (expiring.Count > 0)
            {
                lblAlert.Text = $"⚠️ {expiring.Count} hợp đồng sắp hết hạn trong 30 ngày tới!";
                lblAlert.Height = 35;
            }
            else
            {
                lblAlert.Height = 0;
            }
        }

        private Contract? GetSelected() => dgv.CurrentRow?.DataBoundItem as Contract;

        private async void BtnAdd_Click(object? s, EventArgs e)
        {
            var dlg = new FrmContractDetail(null);
            if (dlg.ShowDialog() == DialogResult.OK) { await LoadDataAsync(); await CheckExpiringAsync(); }
        }

        private async void BtnEdit_Click(object? s, EventArgs e)
        {
            var c = GetSelected();
            if (c == null) { FormHelper.ShowError("Vui lòng chọn hợp đồng."); return; }
            var dlg = new FrmContractDetail(c);
            if (dlg.ShowDialog() == DialogResult.OK) { await LoadDataAsync(); await CheckExpiringAsync(); }
        }

        private async void BtnDelete_Click(object? s, EventArgs e)
        {
            var c = GetSelected();
            if (c == null) { FormHelper.ShowError("Vui lòng chọn hợp đồng."); return; }
            if (!FormHelper.ConfirmDelete($"hợp đồng {c.ContractCode}")) return;
            var (ok, msg) = await Program.ContractService.DeleteAsync(c.ContractId);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
            else FormHelper.ShowError(msg);
        }
    }

    /// <summary>
    /// Dialog thêm/sửa hợp đồng
    /// </summary>
    public class FrmContractDetail : Form
    {
        private ComboBox cboEmployee = null!, cboContractType = null!;
        private DateTimePicker dtpSign = null!, dtpStart = null!, dtpEnd = null!;
        private CheckBox chkNoEnd = null!, chkActive = null!;
        private TextBox txtNotes = null!;
        private readonly Contract? _contract;

        public FrmContractDetail(Contract? contract)
        {
            _contract = contract;
            InitializeComponent();
            this.Load += async (s, e) => await LoadAsync();
        }

        private void InitializeComponent()
        {
            this.Text = _contract == null ? "Thêm Hợp Đồng Mới" : $"Sửa Hợp Đồng - {_contract.ContractCode}";
            this.Size = new Size(480, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            int y = 20, lx = 20, ix = 170, iw = 270;

            AddLabel("Nhân viên: *", lx, y);
            cboEmployee = new ComboBox
            {
                Location = new Point(ix, y), Size = new Size(iw, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground
            };
            if (_contract != null) cboEmployee.Enabled = false;
            this.Controls.Add(cboEmployee); y += 38;

            AddLabel("Loại hợp đồng: *", lx, y);
            cboContractType = new ComboBox
            {
                Location = new Point(ix, y), Size = new Size(iw, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground
            };
            this.Controls.Add(cboContractType); y += 38;

            AddLabel("Ngày ký:", lx, y);
            dtpSign = new DateTimePicker { Location = new Point(ix, y), Size = new Size(150, 30), Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpSign); y += 38;

            AddLabel("Ngày bắt đầu:", lx, y);
            dtpStart = new DateTimePicker { Location = new Point(ix, y), Size = new Size(150, 30), Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpStart); y += 38;

            AddLabel("Ngày kết thúc:", lx, y);
            dtpEnd = new DateTimePicker { Location = new Point(ix, y), Size = new Size(150, 30), Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpEnd);

            chkNoEnd = new CheckBox
            {
                Text = "Không thời hạn", Location = new Point(ix + 160, y + 3),
                AutoSize = true, ForeColor = ThemeColors.MutedForeground
            };
            chkNoEnd.CheckedChanged += (s, e) => dtpEnd.Enabled = !chkNoEnd.Checked;
            this.Controls.Add(chkNoEnd); y += 38;

            chkActive = new CheckBox
            {
                Text = "Còn hiệu lực", Checked = true, Location = new Point(lx, y),
                AutoSize = true, ForeColor = ThemeColors.MutedForeground
            };
            this.Controls.Add(chkActive); y += 32;

            AddLabel("Ghi chú:", lx, y);
            txtNotes = new TextBox
            {
                Location = new Point(ix, y), Size = new Size(iw, 50), Multiline = true,
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground, BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtNotes); y += 60;

            var btnSave = new Button
            {
                Text = "💾 Lưu", Location = new Point(120, y), Size = new Size(100, 38),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Primary,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            var btnCancel = new Button
            {
                Text = "Hủy", Location = new Point(230, y), Size = new Size(80, 38),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.MutedForeground, ForeColor = ThemeColors.Foreground
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label
            {
                Text = text, Location = new Point(x, y + 3), AutoSize = true,
                ForeColor = ThemeColors.MutedForeground
            });
        }

        private async Task LoadAsync()
        {
            // Load employees
            var emps = (await Program.EmployeeService.GetListAsync(1, 500, "", null, true)).Items.ToList();
            cboEmployee.DataSource = emps;
            cboEmployee.DisplayMember = "FullName";
            cboEmployee.ValueMember = "EmployeeId";

            // Load contract types from categories
            var catItems = (await Program.CatService.GetItemsAsync("CONTRACT_TYPE")).ToList();
            cboContractType.DataSource = catItems;
            cboContractType.DisplayMember = "ItemName";
            cboContractType.ValueMember = "ItemCode";

            if (_contract != null)
            {
                cboEmployee.SelectedValue = _contract.EmployeeId;
                cboContractType.SelectedValue = _contract.ContractType;
                dtpSign.Value = _contract.SignDate;
                dtpStart.Value = _contract.StartDate;
                if (_contract.EndDate.HasValue)
                    dtpEnd.Value = _contract.EndDate.Value;
                else
                    chkNoEnd.Checked = true;
                chkActive.Checked = _contract.IsActive;
                txtNotes.Text = _contract.Notes;
            }
        }

        private async void BtnSave_Click(object? s, EventArgs e)
        {
            var contract = _contract ?? new Contract();
            contract.EmployeeId = (int)(cboEmployee.SelectedValue ?? 0);
            contract.ContractType = cboContractType.SelectedValue?.ToString() ?? "";
            contract.SignDate = dtpSign.Value;
            contract.StartDate = dtpStart.Value;
            contract.EndDate = chkNoEnd.Checked ? null : dtpEnd.Value;
            contract.IsActive = chkActive.Checked;
            contract.Notes = txtNotes.Text.Trim();

            (bool ok, string msg) result;
            if (_contract == null)
                result = await Program.ContractService.CreateAsync(contract);
            else
                result = await Program.ContractService.UpdateAsync(contract);

            if (result.ok) { FormHelper.ShowSuccess(result.msg); this.DialogResult = DialogResult.OK; this.Close(); }
            else FormHelper.ShowError(result.msg);
        }
    }
}
