using ClosedXML.Excel;
using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Employee
{
    public class FrmEmployeeList : Form
    {
        private DataGridView dgv = null!;
        private Button btnAdd = null!, btnExport = null!, btnImport = null!, btnBatchEdit = null!;
        private TextBox txtSearch = null!;
        private ComboBox cboDept = null!, cboStatus = null!;
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
            this.Text = "Quản lý nhân sự";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;
            this.Padding = new Padding(32, 24, 32, 24);

            // ===== PAGE HEADER =====
            btnImport = ThemeColors.CreateOutlineButton("📥 Nhập dữ liệu", 140);
            btnImport.Tag = "Add";
            btnExport = ThemeColors.CreateOutlineButton("📤 Xuất dữ liệu", 140);
            btnExport.Tag = "Export";
            btnBatchEdit = ThemeColors.CreateOutlineButton("✏️ Sửa hàng loạt", 150);
            btnBatchEdit.Tag = "Edit";
            btnAdd = ThemeColors.CreatePrimaryButton("+ Thêm mới", 120);
            btnAdd.Tag = "Add";

            var header = ThemeColors.CreatePageHeader(
                "Quản lý nhân sự",
                "Quản lý toàn bộ hồ sơ nhân viên trong hệ thống",
                new Control[] { btnImport, btnExport, btnBatchEdit, btnAdd }
            );
            this.Controls.Add(header);

            btnAdd.Click += BtnAdd_Click;
            btnExport.Click += BtnExportExcel_Click;
            btnImport.Click += BtnImportExcel_Click;
            btnBatchEdit.Click += BtnBatchEdit_Click;

            // ===== FILTER BAR =====
            var filterBar = new Panel
            {
                Dock = DockStyle.Top, Height = 55,
                BackColor = ThemeColors.Background,
                Padding = new Padding(0, 8, 0, 8)
            };

            // Search box
            txtSearch = ThemeColors.CreateSearchBox("Tìm theo tên, mã NV...", 300);
            txtSearch.Location = new Point(0, 8);
            txtSearch.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) { _page = 1; await LoadDataAsync(); } };
            filterBar.Controls.Add(txtSearch);

            // Phòng ban filter
            cboDept = ThemeColors.CreateFilterCombo(180);
            cboDept.Location = new Point(320, 8);
            cboDept.SelectedIndexChanged += async (s, e) => { _page = 1; await LoadDataAsync(); };
            filterBar.Controls.Add(cboDept);

            // Trạng thái filter
            cboStatus = ThemeColors.CreateFilterCombo(160);
            cboStatus.Location = new Point(520, 8);
            cboStatus.Items.AddRange(new object[] { "Tất cả", "Đang làm", "Đã nghỉ" });
            cboStatus.SelectedIndex = 1;
            cboStatus.SelectedIndexChanged += async (s, e) => { _page = 1; await LoadDataAsync(); };
            filterBar.Controls.Add(cboStatus);

            this.Controls.Add(filterBar);

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

            // --- Table Footer (pagination) ---
            var footer = new Panel
            {
                Dock = DockStyle.Bottom, Height = 50,
                BackColor = ThemeColors.Card,
                Padding = new Padding(20, 12, 20, 12)
            };
            footer.Paint += (s, e) =>
            {
                using var pen = new Pen(ThemeColors.Border, 1);
                e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
            };

            lblCount = new Label
            {
                Text = "Đang tải...",
                ForeColor = ThemeColors.MutedForeground,
                Font = ThemeColors.FontSmall,
                Location = new Point(20, 14), AutoSize = true
            };
            footer.Controls.Add(lblCount);

            // Pagination buttons
            var btnPrev = ThemeColors.CreateOutlineButton("‹", 36, 30);
            btnPrev.Location = new Point(footer.Width - 200, 10);
            btnPrev.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPrev.Click += async (s, e) => { if (_page > 1) { _page--; await LoadDataAsync(); } };
            footer.Controls.Add(btnPrev);

            var lblPage = new Label
            {
                Text = "1", ForeColor = ThemeColors.PrimaryForeground,
                BackColor = ThemeColors.Primary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Size = new Size(30, 30), TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(footer.Width - 160, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            footer.Controls.Add(lblPage);

            var btnNext = ThemeColors.CreateOutlineButton("›", 36, 30);
            btnNext.Location = new Point(footer.Width - 120, 10);
            btnNext.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNext.Click += async (s, e) => { _page++; await LoadDataAsync(); };
            footer.Controls.Add(btnNext);

            tableContainer.Controls.Add(footer);

            // --- DataGridView ---
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true
            };
            ThemeColors.StyleDataGridView(dgv);
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };

            tableContainer.Controls.Add(dgv);
            this.Controls.Add(tableContainer);
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
            bool? isActive = cboStatus.SelectedIndex switch
            {
                1 => true,   // Đang làm
                2 => false,  // Đã nghỉ
                _ => null    // Tất cả
            };

            var result = await Program.EmployeeService.GetListAsync(_page, PageSize, txtSearch.Text.Trim(), deptId, isActive);

            dgv.DataSource = null;
            dgv.DataSource = result.Items.ToList();

            // Ẩn cột không cần
            var hideCols = new[] { "EmployeeId", "UserId", "Photo", "DepartmentId", "PositionId", "CreatedAt", "UpdatedAt",
                                   "BankAccount", "BankName", "TaxCode", "InsuranceNo", "SalaryCoefficient", "BasicSalary",
                                   "Notes", "TerminationDate", "Address", "IdentityNo", "DateOfBirth", "Gender" };
            foreach (var col in hideCols)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].Visible = false;

            var headers = new Dictionary<string, string>
            {
                ["EmployeeCode"] = "Mã NV", ["FullName"] = "Họ tên",
                ["DepartmentName"] = "Phòng ban", ["PositionName"] = "Chức vụ",
                ["Phone"] = "Điện thoại", ["Email"] = "Email",
                ["HireDate"] = "Ngày vào làm", ["IsActive"] = "Trạng thái"
            };
            foreach (var (col, h) in headers)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].HeaderText = h;

            lblCount.Text = $"Hiển thị {Math.Min(_page * PageSize, result.TotalCount)}/{result.TotalCount} nhân viên";
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

        private async void BtnBatchEdit_Click(object? s, EventArgs e)
        {
            // Lấy danh sách NV đã chọn
            var selectedIds = new List<int>();
            foreach (DataGridViewRow row in dgv.SelectedRows)
            {
                if (row.DataBoundItem is Models.Entities.Employee emp)
                    selectedIds.Add(emp.EmployeeId);
            }

            if (selectedIds.Count < 2)
            {
                FormHelper.ShowError("Vui lòng chọn ít nhất 2 nhân viên để sửa hàng loạt.\n(Giữ Ctrl + Click để chọn nhiều)");
                return;
            }

            // Dialog chọn trường và giá trị
            using var dlg = new Form
            {
                Text = $"✏️ Sửa hàng loạt ({selectedIds.Count} nhân viên)",
                Size = new Size(420, 280), StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
                BackColor = ThemeColors.Background, Font = new Font("Segoe UI", 10F)
            };

            var lblField = new Label { Text = "Trường cần sửa:", Location = new Point(20, 20), AutoSize = true, ForeColor = ThemeColors.Foreground };
            var cboField = new ComboBox
            {
                Location = new Point(20, 45), Size = new Size(360, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground
            };
            cboField.Items.AddRange(new object[] { "Phòng ban", "Chức vụ", "Hệ số lương", "Số người phụ thuộc" });
            cboField.SelectedIndex = 0;

            var lblValue = new Label { Text = "Giá trị mới:", Location = new Point(20, 90), AutoSize = true, ForeColor = ThemeColors.Foreground };
            var cboValue = new ComboBox
            {
                Location = new Point(20, 115), Size = new Size(360, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground
            };
            var numValue = new NumericUpDown
            {
                Location = new Point(20, 115), Size = new Size(360, 30),
                BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground,
                DecimalPlaces = 2, Minimum = 0, Maximum = 99, Value = 1, Visible = false
            };

            // Load depts and positions
            var depts = (await Program.DeptService.GetAllAsync()).ToList();
            var positions = (await Program.EmployeeService.GetDepartmentsAsync()).ToList();
            var positionsList = (await Program.PosService.GetAllAsync()).ToList();

            void UpdateValueControl()
            {
                var idx = cboField.SelectedIndex;
                if (idx <= 1) // Phòng ban / Chức vụ
                {
                    cboValue.Visible = true; numValue.Visible = false;
                    cboValue.DataSource = null;
                    if (idx == 0)
                    {
                        cboValue.DataSource = depts;
                        cboValue.DisplayMember = "DepartmentName";
                        cboValue.ValueMember = "DepartmentId";
                    }
                    else
                    {
                        cboValue.DataSource = positionsList;
                        cboValue.DisplayMember = "PositionName";
                        cboValue.ValueMember = "PositionId";
                    }
                }
                else // Hệ số lương / Số người phụ thuộc
                {
                    cboValue.Visible = false; numValue.Visible = true;
                    if (idx == 2) { numValue.DecimalPlaces = 2; numValue.Maximum = 99; numValue.Value = 1; }
                    else { numValue.DecimalPlaces = 0; numValue.Maximum = 20; numValue.Value = 0; }
                }
            }
            cboField.SelectedIndexChanged += (_, _) => UpdateValueControl();
            UpdateValueControl();

            var btnOk = new Button
            {
                Text = "✅ Áp dụng", Location = new Point(20, 175), Size = new Size(170, 40),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Primary, ForeColor = ThemeColors.Foreground,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold), DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;
            var btnCancel = new Button
            {
                Text = "Hủy", Location = new Point(210, 175), Size = new Size(170, 40),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderColor = ThemeColors.Border;

            dlg.Controls.AddRange(new Control[] { lblField, cboField, lblValue, cboValue, numValue, btnOk, btnCancel });
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog() != DialogResult.OK) return;

            // Xác định field name và value
            var fieldIdx = cboField.SelectedIndex;
            string fieldName;
            object value;
            switch (fieldIdx)
            {
                case 0: fieldName = "DepartmentId"; value = (int)cboValue.SelectedValue!; break;
                case 1: fieldName = "PositionId"; value = (int)cboValue.SelectedValue!; break;
                case 2: fieldName = "SalaryCoefficient"; value = numValue.Value; break;
                case 3: fieldName = "NumberOfDependents"; value = (int)numValue.Value; break;
                default: return;
            }

            var (ok, msg, count) = await Program.EmployeeService.BatchUpdateAsync(selectedIds, fieldName, value);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadDataAsync(); }
            else FormHelper.ShowError(msg);
        }

        private void BtnExportExcel_Click(object? s, EventArgs e)
        {
            if (dgv.Rows.Count == 0) { FormHelper.ShowError("Không có dữ liệu để xuất."); return; }

            using var dlg = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx", FileName = $"DanhSachNhanVien_{DateTime.Now:yyyyMMdd}",
                Title = "Xuất danh sách nhân viên"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var wb = new XLWorkbook();
                var ws = wb.AddWorksheet("Nhân Viên");

                var visibleCols = new List<(string Name, string Header)>();
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (col.Visible && col.Name != "Select")
                        visibleCols.Add((col.Name, col.HeaderText));
                }
                for (int c = 0; c < visibleCols.Count; c++)
                {
                    ws.Cell(1, c + 1).Value = visibleCols[c].Header;
                    ws.Cell(1, c + 1).Style.Font.Bold = true;
                    ws.Cell(1, c + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(228, 35, 19);
                    ws.Cell(1, c + 1).Style.Font.FontColor = XLColor.White;
                }

                for (int r = 0; r < dgv.Rows.Count; r++)
                {
                    for (int c = 0; c < visibleCols.Count; c++)
                    {
                        var val = dgv.Rows[r].Cells[visibleCols[c].Name].Value;
                        if (val != null) ws.Cell(r + 2, c + 1).Value = val.ToString();
                    }
                }

                ws.Columns().AdjustToContents();
                wb.SaveAs(dlg.FileName);
                FormHelper.ShowSuccess($"Xuất thành công!\n{dlg.FileName}");
            }
            catch (Exception ex)
            {
                FormHelper.ShowError($"Lỗi xuất Excel: {ex.Message}");
            }
        }

        private async void BtnImportExcel_Click(object? s, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx", Title = "Nhập danh sách nhân viên từ Excel"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var wb = new XLWorkbook(dlg.FileName);
                var ws = wb.Worksheet(1);
                var rows = ws.RangeUsed()?.RowsUsed().Skip(1);
                if (rows == null || !rows.Any())
                {
                    FormHelper.ShowError("File Excel không có dữ liệu.");
                    return;
                }

                int imported = 0, errors = 0;
                foreach (var row in rows)
                {
                    try
                    {
                        var emp = new Models.Entities.Employee
                        {
                            FullName = row.Cell(1).GetString().Trim(),
                            Gender = row.Cell(2).GetString().Trim(),
                            DateOfBirth = row.Cell(3).IsEmpty() ? null : row.Cell(3).GetDateTime(),
                            Phone = row.Cell(4).GetString().Trim(),
                            Email = row.Cell(5).GetString().Trim(),
                            Address = row.Cell(6).GetString().Trim(),
                            IdentityNo = row.Cell(7).GetString().Trim()
                        };
                        if (string.IsNullOrWhiteSpace(emp.FullName)) continue;

                        var (ok, _, _) = await Program.EmployeeService.CreateAsync(emp);
                        if (ok) imported++; else errors++;
                    }
                    catch { errors++; }
                }

                FormHelper.ShowSuccess($"Nhập xong!\n✅ Thành công: {imported}\n❌ Lỗi: {errors}");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                FormHelper.ShowError($"Lỗi đọc file Excel: {ex.Message}");
            }
        }
    }
}
