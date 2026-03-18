using QuanLyNhanVien.Helpers;

namespace QuanLyNhanVien.Forms.SystemAdmin
{
    /// <summary>
    /// Nhật ký thao tác: xem AuditLogs với bộ lọc
    /// </summary>
    public class FrmAuditLog : Form
    {
        private DataGridView dgv = null!;
        private DateTimePicker dtpFrom = null!, dtpTo = null!;
        private Button btnSearch = null!, btnRefresh = null!;
        private readonly string _menuCode = "HT_LOG";

        public FrmAuditLog()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Nhật Ký Thao Tác";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // Filter bar
            var filterBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = ThemeColors.Background };

            filterBar.Controls.Add(new Label { Text = "📅 Từ:", Location = new Point(10, 14), AutoSize = true, ForeColor = ThemeColors.Foreground });
            dtpFrom = new DateTimePicker
            {
                Location = new Point(55, 10), Size = new Size(150, 30), Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddDays(-30)
            };
            filterBar.Controls.Add(dtpFrom);

            filterBar.Controls.Add(new Label { Text = "Đến:", Location = new Point(215, 14), AutoSize = true, ForeColor = ThemeColors.Foreground });
            dtpTo = new DateTimePicker
            {
                Location = new Point(255, 10), Size = new Size(150, 30), Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            filterBar.Controls.Add(dtpTo);

            btnSearch = new Button
            {
                Text = "🔍 Tìm", Location = new Point(420, 8), Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Primary,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += async (s, e) => await LoadAsync();
            filterBar.Controls.Add(btnSearch);

            btnRefresh = new Button
            {
                Text = "🔄 Tất cả", Location = new Point(530, 8), Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += async (s, e) =>
            {
                dtpFrom.Value = DateTime.Now.AddDays(-30);
                dtpTo.Value = DateTime.Now;
                await LoadAsync();
            };
            filterBar.Controls.Add(btnRefresh);

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

            this.Controls.Add(dgv);
            this.Controls.Add(filterBar);
        }

        private async Task LoadAsync()
        {
            var logs = (await Program.AuditLogRepo.GetLogsAsync(dtpFrom.Value.Date, dtpTo.Value.Date.AddDays(1))).ToList();

            if (logs.Count == 0)
            {
                dgv.DataSource = null;
                return;
            }

            var dt = new System.Data.DataTable();
            var first = (IDictionary<string, object>)logs[0];
            foreach (var key in first.Keys)
                dt.Columns.Add(key, first[key]?.GetType() ?? typeof(string));

            foreach (var row in logs)
            {
                var dict = (IDictionary<string, object>)row;
                dt.Rows.Add(dict.Values.ToArray());
            }

            dgv.DataSource = dt;

            // Rename headers
            var headerMap = new Dictionary<string, string>
            {
                {"LogId", "ID"}, {"Username", "Người Dùng"}, {"Action", "Hành Động"},
                {"TableName", "Bảng"}, {"RecordId", "Record ID"}, {"OldValue", "Giá Trị Cũ"},
                {"NewValue", "Giá Trị Mới"}, {"IPAddress", "IP"}, {"CreatedAt", "Thời Gian"}
            };
            foreach (var kv in headerMap)
                if (dgv.Columns.Contains(kv.Key)) dgv.Columns[kv.Key].HeaderText = kv.Value;

            if (dgv.Columns.Contains("LogId")) { dgv.Columns["LogId"].Width = 50; }
            if (dgv.Columns.Contains("CreatedAt")) { dgv.Columns["CreatedAt"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm"; dgv.Columns["CreatedAt"].Width = 140; }
        }
    }
}
