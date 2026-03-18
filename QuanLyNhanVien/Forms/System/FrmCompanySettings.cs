using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.Helpers;

namespace QuanLyNhanVien.Forms.SystemAdmin
{
    /// <summary>
    /// Thiết lập thông tin công ty (hiển thị trên báo cáo, phiếu lương)
    /// </summary>
    public class FrmCompanySettings : Form
    {
        private TextBox txtCompanyName = null!, txtAddress = null!, txtPhone = null!;
        private TextBox txtEmail = null!, txtTaxCode = null!, txtWebsite = null!;
        private TextBox txtFax = null!, txtRepresentative = null!, txtRepTitle = null!;
        // SMTP fields
        private TextBox txtSmtpHost = null!, txtSmtpPort = null!, txtSmtpUser = null!;
        private TextBox txtSmtpPass = null!, txtSmtpSenderName = null!, txtSmtpSenderEmail = null!;
        private CheckBox chkSmtpSsl = null!;
        private PictureBox picLogo = null!;
        private Label lblStatus = null!;
        private readonly string _menuCode = "HT_CAUHINH";
        private byte[]? _logoData;

        public FrmCompanySettings()
        {
            InitializeComponent();
            this.Load += async (s, e) => { FormHelper.ApplyPermissions(this, _menuCode); await LoadSettingsAsync(); };
        }

        private void InitializeComponent()
        {
            this.Text = "Thiết Lập Công Ty";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = ThemeColors.Background;

            // === Header ===
            var header = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = ThemeColors.Background };
            header.Controls.Add(new Label
            {
                Text = "🏢 THIẾT LẬP THÔNG TIN CÔNG TY", Dock = DockStyle.Left, AutoSize = false,
                Width = 500, TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Padding = new Padding(15, 0, 0, 0)
            });

            // === Content Panel (scrollable) ===
            var content = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true, BackColor = ThemeColors.Background,
                Padding = new Padding(20, 15, 20, 15)
            };

            int y = 15, labelX = 20, inputX = 200, inputW = 400;

            // Logo
            AddLabel(content, "Logo công ty:", labelX, y);
            picLogo = new PictureBox
            {
                Location = new Point(inputX, y), Size = new Size(120, 120),
                BackColor = ThemeColors.Background, SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand
            };
            picLogo.Click += PicLogo_Click;
            content.Controls.Add(picLogo);

            var lblLogoTip = new Label
            {
                Text = "Click vào ô để chọn logo\n(PNG, JPG, tối đa 2MB)",
                Location = new Point(inputX + 130, y + 20), AutoSize = true,
                ForeColor = Color.FromArgb(120, 130, 160), Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };
            content.Controls.Add(lblLogoTip);
            y += 135;

            // Tên công ty
            AddLabel(content, "Tên công ty: *", labelX, y);
            txtCompanyName = AddTextBox(content, inputX, y, inputW); y += 42;

            // Địa chỉ
            AddLabel(content, "Địa chỉ:", labelX, y);
            txtAddress = AddTextBox(content, inputX, y, inputW); y += 42;

            // SĐT
            AddLabel(content, "Số điện thoại:", labelX, y);
            txtPhone = AddTextBox(content, inputX, y, 200);

            // Fax
            AddLabel(content, "Fax:", labelX + 370, y);
            txtFax = AddTextBox(content, inputX + 370, y, 200); y += 42;

            // Email
            AddLabel(content, "Email:", labelX, y);
            txtEmail = AddTextBox(content, inputX, y, 300); y += 42;

            // Website
            AddLabel(content, "Website:", labelX, y);
            txtWebsite = AddTextBox(content, inputX, y, 300); y += 42;

            // MST
            AddLabel(content, "Mã số thuế:", labelX, y);
            txtTaxCode = AddTextBox(content, inputX, y, 200); y += 42;

            // Đại diện
            AddLabel(content, "Người đại diện:", labelX, y);
            txtRepresentative = AddTextBox(content, inputX, y, 250); y += 42;

            // Chức danh đại diện
            AddLabel(content, "Chức danh:", labelX, y);
            txtRepTitle = AddTextBox(content, inputX, y, 250); y += 55;

            // ===== SMTP Section =====
            var smtpHeader = new Label
            {
                Text = "📧 CẤU HÌNH EMAIL (SMTP)", Location = new Point(labelX, y),
                AutoSize = true, ForeColor = ThemeColors.Foreground,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            content.Controls.Add(smtpHeader); y += 35;

            AddLabel(content, "SMTP Host:", labelX, y);
            txtSmtpHost = AddTextBox(content, inputX, y, 250);
            txtSmtpHost.PlaceholderText = "smtp.gmail.com"; y += 42;

            AddLabel(content, "SMTP Port:", labelX, y);
            txtSmtpPort = AddTextBox(content, inputX, y, 80);
            txtSmtpPort.PlaceholderText = "587";

            chkSmtpSsl = new CheckBox
            {
                Text = "SSL/TLS", Location = new Point(inputX + 120, y + 2),
                AutoSize = true, Checked = true, ForeColor = ThemeColors.Foreground
            };
            content.Controls.Add(chkSmtpSsl); y += 42;

            AddLabel(content, "Tài khoản SMTP:", labelX, y);
            txtSmtpUser = AddTextBox(content, inputX, y, 300);
            txtSmtpUser.PlaceholderText = "your-email@gmail.com"; y += 42;

            AddLabel(content, "Mật khẩu SMTP:", labelX, y);
            txtSmtpPass = AddTextBox(content, inputX, y, 300);
            txtSmtpPass.UseSystemPasswordChar = true;
            txtSmtpPass.PlaceholderText = "App Password"; y += 42;

            AddLabel(content, "Tên người gửi:", labelX, y);
            txtSmtpSenderName = AddTextBox(content, inputX, y, 250);
            txtSmtpSenderName.PlaceholderText = "Phòng Nhân Sự"; y += 42;

            AddLabel(content, "Email người gửi:", labelX, y);
            txtSmtpSenderEmail = AddTextBox(content, inputX, y, 300);
            txtSmtpSenderEmail.PlaceholderText = "hr@company.com (nếu khác tài khoản SMTP)"; y += 50;

            // Buttons
            var btnSave = new Button
            {
                Text = "💾 LƯU THIẾT LẬP", Location = new Point(inputX, y), Size = new Size(180, 42),
                FlatStyle = FlatStyle.Flat, BackColor = ThemeColors.Primary,
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand, Tag = "Edit"
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            content.Controls.Add(btnSave);

            var btnTestEmail = new Button
            {
                Text = "📧 Test Email", Location = new Point(inputX + 195, y), Size = new Size(140, 42),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(168, 85, 247),
                ForeColor = ThemeColors.Foreground, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTestEmail.FlatAppearance.BorderSize = 0;
            btnTestEmail.Click += BtnTestEmail_Click;
            content.Controls.Add(btnTestEmail);

            lblStatus = new Label
            {
                Text = "", Location = new Point(inputX + 350, y + 10), AutoSize = true,
                ForeColor = ThemeColors.Success, Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            content.Controls.Add(lblStatus);

            // WinForms dock order
            this.Controls.Add(content);
            this.Controls.Add(header);
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text, Location = new Point(x, y + 3),
                AutoSize = true, ForeColor = ThemeColors.MutedForeground
            });
        }

        private TextBox AddTextBox(Control parent, int x, int y, int w)
        {
            var tb = new TextBox
            {
                Location = new Point(x, y), Size = new Size(w, 30),
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground,
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(tb);
            return tb;
        }

        private async Task LoadSettingsAsync()
        {
            var settings = await Program.CompanySettingsService.GetAllAsync();

            txtCompanyName.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_COMPANY_NAME, "");
            txtAddress.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_ADDRESS, "");
            txtPhone.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_PHONE, "");
            txtEmail.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_EMAIL, "");
            txtTaxCode.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_TAX_CODE, "");
            txtWebsite.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_WEBSITE, "");
            txtFax.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_FAX, "");
            txtRepresentative.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_REPRESENTATIVE, "");
            txtRepTitle.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_REPRESENTATIVE_TITLE, "");

            // SMTP
            txtSmtpHost.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_SMTP_HOST, "");
            txtSmtpPort.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_SMTP_PORT, "587");
            txtSmtpUser.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_SMTP_USERNAME, "");
            txtSmtpPass.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_SMTP_PASSWORD, "");
            chkSmtpSsl.Checked = settings.GetValueOrDefault(CompanySettingsService.KEY_SMTP_ENABLE_SSL, "true") == "true";
            txtSmtpSenderName.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_SMTP_SENDER_NAME, "");
            txtSmtpSenderEmail.Text = settings.GetValueOrDefault(CompanySettingsService.KEY_SMTP_SENDER_EMAIL, "");

            // Load logo
            var logoBase64 = settings.GetValueOrDefault("Logo", "");
            if (!string.IsNullOrEmpty(logoBase64))
            {
                try
                {
                    _logoData = Convert.FromBase64String(logoBase64);
                    using var ms = new MemoryStream(_logoData);
                    picLogo.Image = Image.FromStream(ms);
                }
                catch { /* Invalid logo data */ }
            }
        }

        private void PicLogo_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Chọn logo công ty"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var fileInfo = new FileInfo(dlg.FileName);
            if (fileInfo.Length > 2 * 1024 * 1024)
            {
                FormHelper.ShowError("File logo không được vượt quá 2MB.");
                return;
            }

            try
            {
                _logoData = File.ReadAllBytes(dlg.FileName);
                using var ms = new MemoryStream(_logoData);
                picLogo.Image = Image.FromStream(ms);
            }
            catch (Exception ex)
            {
                FormHelper.ShowError($"Không thể đọc file ảnh: {ex.Message}");
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            lblStatus.Text = "";

            var settings = new Dictionary<string, string>
            {
                [CompanySettingsService.KEY_COMPANY_NAME] = txtCompanyName.Text.Trim(),
                [CompanySettingsService.KEY_ADDRESS] = txtAddress.Text.Trim(),
                [CompanySettingsService.KEY_PHONE] = txtPhone.Text.Trim(),
                [CompanySettingsService.KEY_EMAIL] = txtEmail.Text.Trim(),
                [CompanySettingsService.KEY_TAX_CODE] = txtTaxCode.Text.Trim(),
                [CompanySettingsService.KEY_WEBSITE] = txtWebsite.Text.Trim(),
                [CompanySettingsService.KEY_FAX] = txtFax.Text.Trim(),
                [CompanySettingsService.KEY_REPRESENTATIVE] = txtRepresentative.Text.Trim(),
                [CompanySettingsService.KEY_REPRESENTATIVE_TITLE] = txtRepTitle.Text.Trim(),
                // SMTP
                [CompanySettingsService.KEY_SMTP_HOST] = txtSmtpHost.Text.Trim(),
                [CompanySettingsService.KEY_SMTP_PORT] = txtSmtpPort.Text.Trim(),
                [CompanySettingsService.KEY_SMTP_USERNAME] = txtSmtpUser.Text.Trim(),
                [CompanySettingsService.KEY_SMTP_PASSWORD] = txtSmtpPass.Text.Trim(),
                [CompanySettingsService.KEY_SMTP_ENABLE_SSL] = chkSmtpSsl.Checked ? "true" : "false",
                [CompanySettingsService.KEY_SMTP_SENDER_NAME] = txtSmtpSenderName.Text.Trim(),
                [CompanySettingsService.KEY_SMTP_SENDER_EMAIL] = txtSmtpSenderEmail.Text.Trim()
            };

            // Save logo as Base64
            if (_logoData != null)
                settings["Logo"] = Convert.ToBase64String(_logoData);

            var (ok, msg) = await Program.CompanySettingsService.SaveAllAsync(settings);

            if (ok)
            {
                lblStatus.ForeColor = ThemeColors.Success;
                lblStatus.Text = "✅ " + msg;
            }
            else
            {
                lblStatus.ForeColor = Color.FromArgb(255, 100, 100);
                lblStatus.Text = "❌ " + msg;
                FormHelper.ShowError(msg);
            }
        }

        private async void BtnTestEmail_Click(object? sender, EventArgs e)
        {
            // Save settings first
            BtnSave_Click(sender, e);
            await Task.Delay(500); // Wait for save

            var testEmail = txtSmtpUser.Text.Trim();
            if (string.IsNullOrWhiteSpace(testEmail))
            {
                FormHelper.ShowError("Vui lòng nhập tài khoản SMTP trước khi test.");
                return;
            }

            lblStatus.Text = "📧 Đang gửi email test...";
            lblStatus.ForeColor = Color.FromArgb(59, 130, 246);

            var (ok, msg) = await Program.EmailService.TestConnectionAsync(testEmail);

            if (ok)
            {
                lblStatus.ForeColor = ThemeColors.Success;
                lblStatus.Text = "✅ " + msg;
                FormHelper.ShowSuccess(msg);
            }
            else
            {
                lblStatus.ForeColor = Color.FromArgb(255, 100, 100);
                lblStatus.Text = "❌ " + msg;
                FormHelper.ShowError(msg);
            }
        }
    }
}
