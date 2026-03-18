using QuanLyNhanVien.Helpers;

namespace QuanLyNhanVien.Forms.Main
{
    public class FrmLogin : Form
    {
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Label lblMessage = null!;

        public FrmLogin()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Đăng Nhập - Hệ thống Quản lý Nhân sự";
            this.Size = new Size(1200, 700);
            this.MinimumSize = new Size(900, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            // ===== BÊN TRÁI: Branding Area (tối) =====
            var panelBrand = new Panel
            {
                Dock = DockStyle.Left, Width = 560,
                BackColor = ThemeColors.SidebarBg,
                Padding = new Padding(60)
            };

            // Logo area
            var panelLogo = new Panel
            {
                Size = new Size(400, 50), Location = new Point(60, 100)
            };
            panelLogo.BackColor = Color.Transparent;
            panelBrand.Controls.Add(panelLogo);

            var iconBox = new Panel
            {
                Size = new Size(48, 48), Location = new Point(0, 0),
                BackColor = ThemeColors.Primary
            };
            iconBox.Paint += (s, e) =>
            {
                using var font = new Font("Segoe UI", 18F, FontStyle.Bold);
                var size = e.Graphics.MeasureString("HR", font);
                e.Graphics.DrawString("HR", font, Brushes.White,
                    (iconBox.Width - size.Width) / 2, (iconBox.Height - size.Height) / 2);
            };
            panelLogo.Controls.Add(iconBox);

            panelLogo.Controls.Add(new Label
            {
                Text = "HRM System", ForeColor = Color.White,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                Location = new Point(60, 6), AutoSize = true
            });

            // Tiêu đề hệ thống
            panelBrand.Controls.Add(new Label
            {
                Text = "Hệ thống\nQuản lý Nhân sự",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                Location = new Point(60, 180), AutoSize = false,
                Size = new Size(440, 100)
            });

            // Mô tả
            panelBrand.Controls.Add(new Label
            {
                Text = "Giải pháp toàn diện cho quản lý nhân sự, chấm công, tính lương và báo cáo doanh nghiệp.",
                ForeColor = ThemeColors.SidebarText,
                Font = new Font("Segoe UI", 12F),
                Location = new Point(60, 280), AutoSize = false,
                Size = new Size(440, 60)
            });

            // Features
            var features = new[]
            {
                "Quản lý hồ sơ nhân viên toàn diện",
                "Tích hợp chấm công và tính lương tự động",
                "Báo cáo thống kê trực quan, realtime"
            };
            for (int i = 0; i < features.Length; i++)
            {
                var featurePanel = new Panel
                {
                    Size = new Size(440, 30),
                    Location = new Point(60, 370 + i * 40),
                    BackColor = Color.Transparent
                };
                featurePanel.Controls.Add(new Label
                {
                    Text = "✓", ForeColor = ThemeColors.Success,
                    Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                    Location = new Point(0, 0), AutoSize = true
                });
                featurePanel.Controls.Add(new Label
                {
                    Text = features[i], ForeColor = ThemeColors.SidebarText,
                    Font = new Font("Segoe UI", 11F),
                    Location = new Point(28, 3), AutoSize = true
                });
                panelBrand.Controls.Add(featurePanel);
            }

            // ===== BÊN PHẢI: Form đăng nhập (sáng) =====
            var panelForm = new Panel
            {
                Dock = DockStyle.Fill, BackColor = ThemeColors.Background,
                Padding = new Padding(80, 60, 80, 60)
            };

            // WinForms dock order: Fill trước, Left sau
            this.Controls.Add(panelForm);
            this.Controls.Add(panelBrand);

            // Container giữa
            var formContainer = new Panel
            {
                Size = new Size(400, 380),
                BackColor = ThemeColors.Background
            };
            panelForm.Controls.Add(formContainer);

            // Center formContainer khi load và khi resize
            void CenterForm()
            {
                formContainer.Location = new Point(
                    Math.Max(0, (panelForm.ClientSize.Width - formContainer.Width) / 2),
                    Math.Max(0, (panelForm.ClientSize.Height - formContainer.Height) / 2 - 20)
                );
            }
            panelForm.Resize += (s, e) => CenterForm();
            this.Load += (s, e) => CenterForm();

            // Title
            formContainer.Controls.Add(new Label
            {
                Text = "Đăng nhập",
                ForeColor = ThemeColors.Foreground,
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                Location = new Point(0, 0), AutoSize = true
            });

            formContainer.Controls.Add(new Label
            {
                Text = "Nhập thông tin tài khoản để truy cập hệ thống",
                ForeColor = ThemeColors.MutedForeground,
                Font = new Font("Segoe UI", 11F),
                Location = new Point(0, 45), AutoSize = true
            });

            // Username field
            formContainer.Controls.Add(new Label
            {
                Text = "Tên đăng nhập", ForeColor = ThemeColors.Foreground,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Location = new Point(0, 100), AutoSize = true
            });

            txtUsername = new TextBox
            {
                Size = new Size(400, 40), Location = new Point(0, 126),
                Font = new Font("Segoe UI", 12F),
                BackColor = ThemeColors.Background,
                ForeColor = ThemeColors.Foreground,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Nhập tên đăng nhập"
            };
            formContainer.Controls.Add(txtUsername);

            // Password field
            var lblPass = new Label
            {
                Text = "Mật khẩu", ForeColor = ThemeColors.Foreground,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Location = new Point(0, 180), AutoSize = true
            };
            formContainer.Controls.Add(lblPass);

            var lblForgot = new Label
            {
                Text = "Quên mật khẩu?",
                ForeColor = ThemeColors.Primary,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(300, 183), AutoSize = true,
                Cursor = Cursors.Hand
            };
            formContainer.Controls.Add(lblForgot);

            txtPassword = new TextBox
            {
                Size = new Size(400, 40), Location = new Point(0, 206),
                Font = new Font("Segoe UI", 12F),
                BackColor = ThemeColors.Background,
                ForeColor = ThemeColors.Foreground,
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●',
                PlaceholderText = "Nhập mật khẩu"
            };
            formContainer.Controls.Add(txtPassword);

            // Message
            lblMessage = new Label
            {
                Text = "", ForeColor = ThemeColors.Error,
                Location = new Point(0, 255), Size = new Size(400, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9.5F)
            };
            formContainer.Controls.Add(lblMessage);

            // Login button
            btnLogin = new Button
            {
                Text = "Đăng nhập",
                Size = new Size(400, 44), Location = new Point(0, 285),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            formContainer.Controls.Add(btnLogin);

            // Copyright
            formContainer.Controls.Add(new Label
            {
                Text = "© 2026 HRM System. All rights reserved.",
                ForeColor = ThemeColors.Placeholder,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(80, 350), AutoSize = true
            });

            // Enter key
            this.AcceptButton = btnLogin;
            txtUsername.Text = "admin";
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            btnLogin.Text = "Đang đăng nhập...";
            lblMessage.Text = "";
            lblMessage.ForeColor = ThemeColors.Error;

            try
            {
                var result = await Program.AuthService.LoginAsync(txtUsername.Text.Trim(), txtPassword.Text);

                if (result.Success && result.User != null)
                {
                    AppSession.CurrentUser = result.User;
                    AppSession.Permissions = result.Permissions;
                    AppSession.Roles = result.Roles;

                    lblMessage.ForeColor = ThemeColors.Success;
                    lblMessage.Text = result.Message;

                    await Task.Delay(500);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblMessage.Text = result.Message;
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Lỗi kết nối: {ex.Message}";
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Đăng nhập";
            }
        }
    }
}
