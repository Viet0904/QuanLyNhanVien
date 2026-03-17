using QuanLyNhanVien.Helpers;

namespace QuanLyNhanVien.Forms.Main
{
    public class FrmLogin : Form
    {
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Label lblMessage = null!;
        private PictureBox picLogo = null!;

        public FrmLogin()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Đăng Nhập - Quản Lý Nhân Viên";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 46);
            this.Font = new Font("Segoe UI", 10F);

            // Panel chính
            var panel = new Panel
            {
                Size = new Size(380, 300),
                Location = new Point(25, 30),
                BackColor = Color.FromArgb(45, 45, 65)
            };
            panel.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                using var pen = new Pen(Color.FromArgb(100, 100, 180), 1);
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, Color.FromArgb(50, 50, 75), Color.FromArgb(40, 40, 60),
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(brush, rect);
                e.Graphics.DrawRectangle(pen, rect);
            };
            this.Controls.Add(panel);

            // Title
            var lblTitle = new Label
            {
                Text = "🔐 ĐĂNG NHẬP HỆ THỐNG",
                ForeColor = Color.FromArgb(200, 210, 255),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(340, 40),
                Location = new Point(20, 20)
            };
            panel.Controls.Add(lblTitle);

            // Username
            var lblUser = new Label
            {
                Text = "Tên đăng nhập",
                ForeColor = Color.FromArgb(180, 180, 210),
                Location = new Point(30, 75),
                AutoSize = true
            };
            panel.Controls.Add(lblUser);

            txtUsername = new TextBox
            {
                Size = new Size(320, 35),
                Location = new Point(30, 98),
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(55, 55, 80),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.Controls.Add(txtUsername);

            // Password
            var lblPass = new Label
            {
                Text = "Mật khẩu",
                ForeColor = Color.FromArgb(180, 180, 210),
                Location = new Point(30, 140),
                AutoSize = true
            };
            panel.Controls.Add(lblPass);

            txtPassword = new TextBox
            {
                Size = new Size(320, 35),
                Location = new Point(30, 163),
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(55, 55, 80),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●'
            };
            panel.Controls.Add(txtPassword);

            // Message
            lblMessage = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(255, 100, 100),
                Location = new Point(30, 205),
                Size = new Size(320, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F)
            };
            panel.Controls.Add(lblMessage);

            // Login button
            btnLogin = new Button
            {
                Text = "ĐĂNG NHẬP",
                Size = new Size(320, 45),
                Location = new Point(30, 230),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(88, 101, 242),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            panel.Controls.Add(btnLogin);

            // Enter key
            this.AcceptButton = btnLogin;
            txtUsername.Text = "admin";
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            btnLogin.Text = "Đang đăng nhập...";
            lblMessage.Text = "";
            lblMessage.ForeColor = Color.FromArgb(255, 100, 100);

            try
            {
                var result = await Program.AuthService.LoginAsync(txtUsername.Text.Trim(), txtPassword.Text);

                if (result.Success && result.User != null)
                {
                    AppSession.CurrentUser = result.User;
                    AppSession.Permissions = result.Permissions;
                    AppSession.Roles = result.Roles;

                    lblMessage.ForeColor = Color.FromArgb(100, 255, 100);
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
                btnLogin.Text = "ĐĂNG NHẬP";
            }
        }
    }
}
