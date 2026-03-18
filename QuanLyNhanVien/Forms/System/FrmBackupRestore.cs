using QuanLyNhanVien.Helpers;

namespace QuanLyNhanVien.Forms.SystemAdmin
{
    /// <summary>
    /// Sao lưu & Phục hồi Database
    /// </summary>
    public class FrmBackupRestore : Form
    {
        private TextBox txtBackupFolder = null!;
        private DataGridView dgv = null!;
        private Label lblStatus = null!;
        private readonly string _menuCode = "HT_BACKUP";
        private string _backupFolder = string.Empty;

        public FrmBackupRestore()
        {
            InitializeComponent();
            this.Load += (s, e) =>
            {
                FormHelper.ApplyPermissions(this, _menuCode);
                // Mặc định folder backup trong thư mục app
                _backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                txtBackupFolder.Text = _backupFolder;
                RefreshBackupList();
            };
        }

        private void InitializeComponent()
        {
            this.Text = "Sao Lưu & Phục Hồi Database";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // === Header ===
            var header = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = ThemeColors.Background };
            header.Controls.Add(new Label
            {
                Text = "💾 SAO LƯU & PHỤC HỒI DATABASE", Dock = DockStyle.Left, AutoSize = false,
                Width = 500, TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Padding = new Padding(15, 0, 0, 0)
            });

            // === Backup Panel ===
            var backupPanel = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Color.FromArgb(38, 38, 55), Padding = new Padding(15, 10, 15, 10) };
            backupPanel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(60, 60, 90), 1);
                e.Graphics.DrawLine(pen, 0, backupPanel.Height - 1, backupPanel.Width, backupPanel.Height - 1);
            };

            backupPanel.Controls.Add(new Label
            {
                Text = "📂 Thư mục sao lưu:", Location = new Point(15, 15),
                AutoSize = true, ForeColor = ThemeColors.MutedForeground
            });

            txtBackupFolder = new TextBox
            {
                Location = new Point(175, 12), Size = new Size(450, 30),
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground,
                BorderStyle = BorderStyle.FixedSingle, ReadOnly = true
            };
            backupPanel.Controls.Add(txtBackupFolder);

            var btnBrowse = new Button
            {
                Text = "📁 Chọn", Location = new Point(635, 10), Size = new Size(80, 32),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(80, 80, 110),
                ForeColor = ThemeColors.Foreground, Cursor = Cursors.Hand
            };
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.Click += (s, e) =>
            {
                using var dlg = new FolderBrowserDialog { Description = "Chọn thư mục sao lưu" };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _backupFolder = dlg.SelectedPath;
                    txtBackupFolder.Text = _backupFolder;
                    RefreshBackupList();
                }
            };
            backupPanel.Controls.Add(btnBrowse);

            var btnBackup = new Button
            {
                Text = "⬇️ SAO LƯU NGAY", Location = new Point(15, 55), Size = new Size(200, 40),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Primary,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand,
                Tag = "Add"
            };
            btnBackup.FlatAppearance.BorderSize = 0;
            btnBackup.Click += BtnBackup_Click;
            backupPanel.Controls.Add(btnBackup);

            var btnRestore = new Button
            {
                Text = "⬆️ PHỤC HỒI TỪ FILE", Location = new Point(230, 55), Size = new Size(200, 40),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(200, 120, 20),
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand,
                Tag = "Edit"
            };
            btnRestore.FlatAppearance.BorderSize = 0;
            btnRestore.Click += BtnRestore_Click;
            backupPanel.Controls.Add(btnRestore);

            lblStatus = new Label
            {
                Text = "", Location = new Point(450, 65), AutoSize = true,
                ForeColor = ThemeColors.Success, Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            backupPanel.Controls.Add(lblStatus);

            // === Danh sách backup ===
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
                ColumnHeadersHeight = 38, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };

            // WinForms dock order
            this.Controls.Add(dgv);
            this.Controls.Add(backupPanel);
            this.Controls.Add(header);
        }

        private void RefreshBackupList()
        {
            var files = Program.BackupService.GetBackupFiles(_backupFolder);
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Tên File", typeof(string));
            dt.Columns.Add("Kích Thước (MB)", typeof(string));
            dt.Columns.Add("Ngày Tạo", typeof(DateTime));

            foreach (var (name, size, created) in files)
                dt.Rows.Add(name, $"{size / (1024.0 * 1024.0):F2}", created);

            dgv.DataSource = dt;
            if (dgv.Columns.Contains("Ngày Tạo"))
                dgv.Columns["Ngày Tạo"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
        }

        private async void BtnBackup_Click(object? s, EventArgs e)
        {
            if (string.IsNullOrEmpty(_backupFolder))
            {
                FormHelper.ShowError("Vui lòng chọn thư mục sao lưu.");
                return;
            }

            lblStatus.ForeColor = ThemeColors.Warning;
            lblStatus.Text = "⏳ Đang sao lưu...";
            this.Cursor = Cursors.WaitCursor;

            var (ok, msg) = await Program.BackupService.BackupAsync(_backupFolder);

            this.Cursor = Cursors.Default;
            if (ok)
            {
                lblStatus.ForeColor = ThemeColors.Success;
                lblStatus.Text = "✅ Sao lưu thành công!";
                FormHelper.ShowSuccess(msg);
                RefreshBackupList();
            }
            else
            {
                lblStatus.ForeColor = Color.FromArgb(255, 100, 100);
                lblStatus.Text = "❌ Sao lưu thất bại!";
                FormHelper.ShowError(msg);
            }
        }

        private async void BtnRestore_Click(object? s, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Backup Files|*.bak",
                Title = "Chọn file backup để phục hồi",
                InitialDirectory = _backupFolder
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var confirm = MessageBox.Show(
                "⚠️ CẢNH BÁO: Phục hồi database sẽ GHI ĐÈ toàn bộ dữ liệu hiện tại!\n\n" +
                $"File: {Path.GetFileName(dlg.FileName)}\n\n" +
                "Bạn có chắc chắn muốn tiếp tục?",
                "Xác nhận phục hồi",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            lblStatus.ForeColor = ThemeColors.Warning;
            lblStatus.Text = "⏳ Đang phục hồi...";
            this.Cursor = Cursors.WaitCursor;

            var (ok, msg) = await Program.BackupService.RestoreAsync(dlg.FileName);

            this.Cursor = Cursors.Default;
            if (ok)
            {
                lblStatus.ForeColor = ThemeColors.Success;
                lblStatus.Text = "✅ Phục hồi thành công!";
                FormHelper.ShowSuccess(msg);
            }
            else
            {
                lblStatus.ForeColor = Color.FromArgb(255, 100, 100);
                lblStatus.Text = "❌ Phục hồi thất bại!";
                FormHelper.ShowError(msg);
            }
        }
    }
}
