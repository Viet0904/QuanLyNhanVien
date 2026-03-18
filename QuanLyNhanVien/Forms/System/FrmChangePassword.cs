using QuanLyNhanVien.Helpers;

namespace QuanLyNhanVien.Forms.SystemAdmin
{
    /// <summary>
    /// Dialog đổi mật khẩu
    /// </summary>
    public class FrmChangePassword : Form
    {
        private TextBox txtOldPass = null!, txtNewPass = null!, txtConfirm = null!;
        private Label lblMessage = null!;
        private Button btnSave = null!, btnCancel = null!;

        public FrmChangePassword()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "🔑 Đổi Mật Khẩu";
            this.Size = new Size(420, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = new Font("Segoe UI", 10F);

            var panel = new Panel
            {
                Location = new Point(20, 15),
                Size = new Size(365, 250),
                BackColor = Color.FromArgb(45, 45, 65)
            };
            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(80, 80, 120), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };
            this.Controls.Add(panel);

            var y = 20;

            // Mật khẩu cũ
            panel.Controls.Add(new Label
            {
                Text = "Mật khẩu hiện tại:", Location = new Point(20, y + 3),
                AutoSize = true, ForeColor = ThemeColors.MutedForeground
            });
            y += 28;
            txtOldPass = CreatePasswordBox(panel, y); y += 42;

            // Mật khẩu mới
            panel.Controls.Add(new Label
            {
                Text = "Mật khẩu mới:", Location = new Point(20, y + 3),
                AutoSize = true, ForeColor = ThemeColors.MutedForeground
            });
            y += 28;
            txtNewPass = CreatePasswordBox(panel, y); y += 42;

            // Xác nhận
            panel.Controls.Add(new Label
            {
                Text = "Xác nhận mật khẩu mới:", Location = new Point(20, y + 3),
                AutoSize = true, ForeColor = ThemeColors.MutedForeground
            });
            y += 28;
            txtConfirm = CreatePasswordBox(panel, y); y += 42;

            // Message
            lblMessage = new Label
            {
                Text = "", Location = new Point(20, y),
                Size = new Size(325, 20), ForeColor = Color.FromArgb(255, 100, 100),
                Font = new Font("Segoe UI", 9F), TextAlign = ContentAlignment.MiddleCenter
            };
            panel.Controls.Add(lblMessage);

            // Buttons
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 50, BackColor = ThemeColors.Background,
                Padding = new Padding(90, 8, 0, 0), FlowDirection = FlowDirection.LeftToRight
            };

            btnSave = new Button
            {
                Text = "💾 Lưu", Size = new Size(110, 36), FlatStyle = FlatStyle.Flat,
                BackColor = ThemeColors.Primary, ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Hủy", Size = new Size(90, 36), FlatStyle = FlatStyle.Flat,
                BackColor = ThemeColors.MutedForeground, ForeColor = ThemeColors.Foreground,
                Margin = new Padding(10, 0, 0, 0), Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            btnPanel.Controls.AddRange(new Control[] { btnSave, btnCancel });
            this.Controls.Add(btnPanel);

            this.AcceptButton = btnSave;
            txtOldPass.Focus();
        }

        private TextBox CreatePasswordBox(Control parent, int y)
        {
            var tb = new TextBox
            {
                Location = new Point(20, y), Size = new Size(325, 32),
                BackColor = ThemeColors.Background, ForeColor = ThemeColors.Foreground,
                BorderStyle = BorderStyle.FixedSingle, PasswordChar = '●',
                Font = new Font("Segoe UI", 11F)
            };
            parent.Controls.Add(tb);
            return tb;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            lblMessage.Text = "";
            lblMessage.ForeColor = Color.FromArgb(255, 100, 100);

            if (string.IsNullOrWhiteSpace(txtOldPass.Text))
            {
                lblMessage.Text = "Vui lòng nhập mật khẩu hiện tại.";
                txtOldPass.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNewPass.Text))
            {
                lblMessage.Text = "Vui lòng nhập mật khẩu mới.";
                txtNewPass.Focus();
                return;
            }

            if (txtNewPass.Text != txtConfirm.Text)
            {
                lblMessage.Text = "Mật khẩu xác nhận không khớp.";
                txtConfirm.Focus();
                return;
            }

            if (txtNewPass.Text == txtOldPass.Text)
            {
                lblMessage.Text = "Mật khẩu mới phải khác mật khẩu cũ.";
                txtNewPass.Focus();
                return;
            }

            btnSave.Enabled = false;
            btnSave.Text = "Đang lưu...";

            try
            {
                var userId = AppSession.CurrentUser?.UserId ?? 0;
                var (ok, msg) = await Program.AuthService.ChangePasswordAsync(userId, txtOldPass.Text, txtNewPass.Text);

                if (ok)
                {
                    lblMessage.ForeColor = Color.FromArgb(100, 255, 100);
                    lblMessage.Text = msg;
                    await Task.Delay(800);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblMessage.Text = msg;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Lỗi: {ex.Message}";
            }
            finally
            {
                btnSave.Enabled = true;
                btnSave.Text = "💾 Lưu";
            }
        }
    }
}
