using ClosedXML.Excel;
using QuanLyNhanVien.Helpers;

namespace QuanLyNhanVien.Forms.Report
{
    /// <summary>
    /// Xuất báo cáo: chọn loại → preview → CSV / Print
    /// </summary>
    public class FrmReportExport : Form
    {
        private ComboBox cboReportType = null!, cboMonth = null!, cboYear = null!, cboDept = null!;
        private DataGridView dgv = null!;
        private Label lblInfo = null!;
        private readonly string _menuCode = "BC_VIEWER";

        public FrmReportExport()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadFiltersAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Xuất Báo Cáo";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // Toolbar
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background,
                Padding = new Padding(8, 8, 0, 0)
            };
            var btnPreview = CreateBtn("👁 Xem trước", ThemeColors.Primary);
            btnPreview.Click += async (s, e) => await PreviewAsync();
            var btnExport = CreateBtn("📥 Xuất CSV", ThemeColors.Success);
            btnExport.Tag = "Export";
            btnExport.Click += BtnExport_Click;
            var btnExcel = CreateBtn("📊 Xuất Excel", ThemeColors.Success);
            btnExcel.Tag = "Export";
            btnExcel.Click += BtnExportExcel_Click;
            var btnPrint = CreateBtn("🖨 In", Color.FromArgb(168, 85, 247));
            btnPrint.Tag = "Print";
            btnPrint.Click += BtnPrint_Click;
            toolbar.Controls.AddRange(new Control[] { btnPreview, btnExport, btnExcel, btnPrint });

            // Filter bar
            var filterBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background };

            filterBar.Controls.Add(new Label { Text = "📋 Loại:", Location = new Point(10, 14), AutoSize = true, ForeColor = ThemeColors.Foreground });
            cboReportType = new ComboBox
            {
                Location = new Point(65, 10), Size = new Size(180, 30), DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat
            };
            cboReportType.Items.AddRange(new object[] {
                "Danh sách nhân viên", "Bảng lương tháng", "Chấm công tháng", "Nghỉ phép",
                "NV mới / nghỉ việc", "Sinh nhật tháng", "Bảo hiểm xã hội", "Thuế TNCN", "Biến động nhân sự"
            });
            cboReportType.SelectedIndex = 0;
            cboReportType.SelectedIndexChanged += (s, e) => UpdateFilterVisibility();
            filterBar.Controls.Add(cboReportType);

            filterBar.Controls.Add(new Label { Text = "Tháng:", Location = new Point(260, 14), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
            cboMonth = new ComboBox { Location = new Point(315, 10), Size = new Size(60, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            for (int i = 1; i <= 12; i++) cboMonth.Items.Add(i);
            cboMonth.SelectedItem = DateTime.Now.Month;
            filterBar.Controls.Add(cboMonth);

            filterBar.Controls.Add(new Label { Text = "Năm:", Location = new Point(385, 14), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
            cboYear = new ComboBox { Location = new Point(425, 10), Size = new Size(80, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground };
            for (int y = DateTime.Now.Year; y >= DateTime.Now.Year - 5; y--) cboYear.Items.Add(y);
            cboYear.SelectedIndex = 0;
            filterBar.Controls.Add(cboYear);

            filterBar.Controls.Add(new Label { Text = "P.Ban:", Location = new Point(520, 14), AutoSize = true, ForeColor = ThemeColors.MutedForeground });
            cboDept = new ComboBox { Location = new Point(570, 10), Size = new Size(170, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = ThemeColors.Surface, ForeColor = ThemeColors.Foreground, FlatStyle = FlatStyle.Flat };
            filterBar.Controls.Add(cboDept);

            lblInfo = new Label { Text = "", Location = new Point(755, 14), AutoSize = true, ForeColor = ThemeColors.Success, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            filterBar.Controls.Add(lblInfo);

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

            // WinForms dock order
            this.Controls.Add(dgv);
            this.Controls.Add(filterBar);
            this.Controls.Add(toolbar);
        }

        private Button CreateBtn(string text, Color c)
        {
            var b = new Button
            {
                Text = text, Size = new Size(130, 35), FlatStyle = FlatStyle.Flat,
                BackColor = c, ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
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
            UpdateFilterVisibility();
        }

        private void UpdateFilterVisibility()
        {
            var idx = cboReportType.SelectedIndex;
            cboMonth.Visible = idx >= 1 && idx != 3 && idx != 8;
            cboYear.Visible = idx >= 1;
        }

        private async Task PreviewAsync()
        {
            var idx = cboReportType.SelectedIndex;
            var month = cboMonth.SelectedItem is int m ? m : DateTime.Now.Month;
            var year = cboYear.SelectedItem is int yr ? yr : DateTime.Now.Year;
            var deptId = cboDept.SelectedValue is int d && d > 0 ? d : (int?)null;

            IEnumerable<dynamic> data;

            switch (idx)
            {
                case 0: // Danh sách NV
                    data = await Program.ReportService.GetEmployeeReportAsync(deptId);
                    break;
                case 1: // Bảng lương
                    data = await Program.ReportService.GetSalaryReportAsync(month, year, deptId);
                    break;
                case 2: // Chấm công
                    data = await Program.ReportService.GetAttendanceSummaryAsync(month, year);
                    break;
                case 3: // Nghỉ phép
                    var leaveStats = await Program.ReportService.GetLeaveStatisticsAsync(year);
                    data = leaveStats.Select(l => (dynamic)new { TrangThai = l.Status, SoLuong = l.Count }).ToList();
                    break;
                case 4: // NV mới / nghỉ việc
                    data = await Program.ReportRepo.GetNewTerminatedReportAsync(month, year);
                    break;
                case 5: // Sinh nhật
                    data = await Program.ReportRepo.GetBirthdayReportAsync(month);
                    break;
                case 6: // BHXH
                    data = await Program.ReportRepo.GetInsuranceReportAsync(month, year);
                    break;
                case 7: // Thuế TNCN
                    data = await Program.ReportRepo.GetTaxReportAsync(month, year);
                    break;
                case 8: // Biến động nhân sự
                    var turnover = await Program.ReportRepo.GetTurnoverAsync(year);
                    data = turnover.Select(t => (dynamic)new { Thang = t.Month, NhanVienMoi = t.NewHires, NghiViec = t.Terminations }).ToList();
                    break;
                default:
                    return;
            }

            var list = data.ToList();
            if (list.Count == 0)
            {
                dgv.DataSource = null;
                lblInfo.Text = "Không có dữ liệu.";
                return;
            }

            // Convert dynamic to DataTable for DataGridView
            var dt = new System.Data.DataTable();
            var first = (IDictionary<string, object>)list[0];
            foreach (var key in first.Keys)
                dt.Columns.Add(key, first[key]?.GetType() ?? typeof(string));

            foreach (var row in list)
            {
                var dict = (IDictionary<string, object>)row;
                dt.Rows.Add(dict.Values.ToArray());
            }

            dgv.DataSource = dt;
            lblInfo.Text = $"📊 {list.Count} dòng";

            // Format money columns
            var moneyCols = new[] { "BasicSalary", "SalaryCoefficient", "PositionAllowance", "OvertimePay", "GrossIncome",
                "SocialInsurance", "HealthInsurance", "UnemploymentInsurance", "PersonalIncomeTax", "NetSalary" };
            foreach (var col in moneyCols)
                if (dgv.Columns.Contains(col)) dgv.Columns[col].DefaultCellStyle.Format = "N0";
        }

        private void BtnExport_Click(object? s, EventArgs e)
        {
            if (dgv.DataSource == null || dgv.Rows.Count == 0)
            {
                FormHelper.ShowError("Chưa có dữ liệu để xuất. Hãy nhấn 'Xem trước' trước.");
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Filter = "CSV Files|*.csv",
                Title = "Xuất báo cáo CSV",
                FileName = $"BaoCao_{cboReportType.Text.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var sw = new StreamWriter(dlg.FileName, false, System.Text.Encoding.UTF8);

                // Header
                var headers = new List<string>();
                foreach (DataGridViewColumn col in dgv.Columns)
                    if (col.Visible) headers.Add($"\"{col.HeaderText}\"");
                sw.WriteLine(string.Join(",", headers));

                // Data
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    var cells = new List<string>();
                    foreach (DataGridViewColumn col in dgv.Columns)
                    {
                        if (!col.Visible) continue;
                        var val = row.Cells[col.Index].Value?.ToString()?.Replace("\"", "\"\"") ?? "";
                        cells.Add($"\"{val}\"");
                    }
                    sw.WriteLine(string.Join(",", cells));
                }

                FormHelper.ShowSuccess($"Đã xuất thành công!\n{dlg.FileName}");
            }
            catch (Exception ex)
            {
                FormHelper.ShowError($"Lỗi xuất file: {ex.Message}");
            }
        }

        private void BtnExportExcel_Click(object? s, EventArgs e)
        {
            if (dgv.DataSource == null || dgv.Rows.Count == 0)
            {
                FormHelper.ShowError("Chưa có dữ liệu để xuất. Hãy nhấn 'Xem trước' trước.");
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Xuất báo cáo Excel",
                FileName = $"BaoCao_{cboReportType.Text.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var wb = new XLWorkbook();
                var ws = wb.AddWorksheet("Báo Cáo");

                var visibleCols = new List<(int Index, string Header)>();
                foreach (DataGridViewColumn col in dgv.Columns)
                    if (col.Visible) visibleCols.Add((col.Index, col.HeaderText));

                for (int c = 0; c < visibleCols.Count; c++)
                {
                    ws.Cell(1, c + 1).Value = visibleCols[c].Header;
                    ws.Cell(1, c + 1).Style.Font.Bold = true;
                    ws.Cell(1, c + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(228, 35, 19);
                    ws.Cell(1, c + 1).Style.Font.FontColor = XLColor.White;
                }

                for (int r = 0; r < dgv.Rows.Count; r++)
                    for (int c = 0; c < visibleCols.Count; c++)
                    {
                        var val = dgv.Rows[r].Cells[visibleCols[c].Index].Value;
                        if (val != null) ws.Cell(r + 2, c + 1).Value = val.ToString();
                    }

                ws.Columns().AdjustToContents();
                wb.SaveAs(dlg.FileName);
                FormHelper.ShowSuccess($"Đã xuất thành công!\n{dlg.FileName}");
            }
            catch (Exception ex)
            {
                FormHelper.ShowError($"Lỗi xuất Excel: {ex.Message}");
            }
        }

        private void BtnPrint_Click(object? s, EventArgs e)
        {
            if (dgv.DataSource == null || dgv.Rows.Count == 0)
            {
                FormHelper.ShowError("Chưa có dữ liệu để in. Hãy nhấn 'Xem trước' trước.");
                return;
            }

            var printDoc = new System.Drawing.Printing.PrintDocument();
            printDoc.PrintPage += (sender, pe) =>
            {
                if (pe.Graphics == null) return;
                var g = pe.Graphics;
                var font = new Font("Segoe UI", 9F);
                var headerFont = new Font("Segoe UI", 10F, FontStyle.Bold);
                int y = 50;

                // Title
                g.DrawString($"BÁO CÁO: {cboReportType.Text}", new Font("Segoe UI", 14F, FontStyle.Bold), Brushes.Black, 50, y);
                y += 40;
                g.DrawString($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}", font, Brushes.Gray, 50, y);
                y += 30;

                // Column headers
                int x = 50;
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (!col.Visible) continue;
                    g.DrawString(col.HeaderText, headerFont, Brushes.Black, x, y);
                    x += col.Width;
                }
                y += 25;
                g.DrawLine(Pens.Black, 50, y, pe.PageBounds.Width - 50, y);
                y += 5;

                // Data rows
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (y > pe.PageBounds.Height - 80) break;
                    x = 50;
                    foreach (DataGridViewColumn col in dgv.Columns)
                    {
                        if (!col.Visible) continue;
                        var val = row.Cells[col.Index].Value?.ToString() ?? "";
                        g.DrawString(val, font, Brushes.Black, x, y);
                        x += col.Width;
                    }
                    y += 22;
                }

                font.Dispose();
                headerFont.Dispose();
            };

            using var preview = new PrintPreviewDialog { Document = printDoc, Width = 800, Height = 600 };
            preview.ShowDialog();
        }
    }
}
