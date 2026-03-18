namespace QuanLyNhanVien.Helpers
{
    /// <summary>
    /// Hệ thống màu sắc và style theo thiết kế Pencil.
    /// Nền sáng + sidebar tối + primary đỏ.
    /// </summary>
    public static class ThemeColors
    {
        // ===== Màu nền =====
        public static readonly Color Background = Color.FromArgb(255, 255, 255);     // #FFFFFF
        public static readonly Color Surface = Color.FromArgb(250, 250, 250);        // #FAFAFA
        public static readonly Color Card = Color.FromArgb(255, 255, 255);           // #FFFFFF

        // ===== Màu chữ =====
        public static readonly Color Foreground = Color.FromArgb(13, 13, 13);        // #0D0D0D
        public static readonly Color MutedForeground = Color.FromArgb(122, 122, 122);// #7A7A7A
        public static readonly Color Placeholder = Color.FromArgb(176, 176, 176);    // #B0B0B0

        // ===== Màu viền =====
        public static readonly Color Border = Color.FromArgb(232, 232, 232);         // #E8E8E8

        // ===== Màu chính =====
        public static readonly Color Primary = Color.FromArgb(228, 35, 19);          // #E42313
        public static readonly Color PrimaryForeground = Color.FromArgb(255, 255, 255);

        // ===== Sidebar =====
        public static readonly Color SidebarBg = Color.FromArgb(26, 26, 46);         // #1A1A2E
        public static readonly Color SidebarText = Color.FromArgb(160, 160, 184);    // #A0A0B8
        public static readonly Color SidebarHover = Color.FromArgb(46, 46, 76);      // #2E2E4C - sáng hơn nền
        public static readonly Color SidebarActiveBg = Color.FromArgb(228, 35, 19, 25); // Primary với opacity thấp
        public static readonly Color SidebarActive = Color.FromArgb(255, 255, 255);  // #FFFFFF
        public static readonly Color SidebarDivider = Color.FromArgb(42, 42, 74);    // #2A2A4A

        // ===== Trạng thái =====
        public static readonly Color Success = Color.FromArgb(34, 197, 94);          // #22C55E
        public static readonly Color Warning = Color.FromArgb(245, 158, 11);         // #F59E0B
        public static readonly Color Error = Color.FromArgb(239, 68, 68);            // #EF4444
        public static readonly Color Info = Color.FromArgb(59, 130, 246);            // #3B82F6

        // ===== Badge backgrounds =====
        public static readonly Color SuccessBg = Color.FromArgb(220, 252, 231);      // xanh nhạt
        public static readonly Color WarningBg = Color.FromArgb(254, 243, 199);      // vàng nhạt
        public static readonly Color ErrorBg = Color.FromArgb(254, 226, 226);        // đỏ nhạt
        public static readonly Color InfoBg = Color.FromArgb(219, 234, 254);         // xanh dương nhạt

        // ===== Fonts =====
        public static readonly Font FontTitle = new("Segoe UI", 24F, FontStyle.Bold);
        public static readonly Font FontPageTitle = new("Segoe UI", 18F, FontStyle.Bold);
        public static readonly Font FontSubtitle = new("Segoe UI", 12F);
        public static readonly Font FontBody = new("Segoe UI", 10F);
        public static readonly Font FontSmall = new("Segoe UI", 9F);
        public static readonly Font FontBold = new("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font FontMetricValue = new("Segoe UI", 28F, FontStyle.Bold);
        public static readonly Font FontMetricLabel = new("Segoe UI", 12F);
        public static readonly Font FontSidebarItem = new("Segoe UI", 10.5F);
        public static readonly Font FontSidebarLogo = new("Segoe UI", 14F, FontStyle.Bold);
        public static readonly Font FontTableHeader = new("Segoe UI", 9.5F, FontStyle.Bold);
        public static readonly Font FontTableCell = new("Segoe UI", 9.5F);

        // ===== Helper: Tạo button primary (đỏ) =====
        public static Button CreatePrimaryButton(string text, int width = 120, int height = 36)
        {
            var btn = new Button
            {
                Text = text, Size = new Size(width, height),
                FlatStyle = FlatStyle.Flat, BackColor = Primary,
                ForeColor = PrimaryForeground, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ===== Helper: Tạo button outline =====
        public static Button CreateOutlineButton(string text, int width = 120, int height = 36)
        {
            var btn = new Button
            {
                Text = text, Size = new Size(width, height),
                FlatStyle = FlatStyle.Flat, BackColor = Background,
                ForeColor = Foreground, Font = new Font("Segoe UI", 9.5F),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 190); // #BEBEBE - viền rõ ràng
            btn.FlatAppearance.BorderSize = 1;
            return btn;
        }

        // ===== Helper: Áp dụng theme sáng cho DataGridView =====
        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = Background;
            dgv.GridColor = Border;
            dgv.BorderStyle = BorderStyle.None;
            dgv.RowHeadersVisible = false;
            dgv.EnableHeadersVisualStyles = false;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Background,
                ForeColor = Foreground,
                SelectionBackColor = Color.FromArgb(239, 246, 255), // xanh rất nhạt
                SelectionForeColor = Foreground,
                Font = FontTableCell,
                Padding = new Padding(8, 4, 8, 4)
            };

            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Surface,
                ForeColor = MutedForeground,
                Font = FontTableHeader,
                Padding = new Padding(8, 6, 8, 6)
            };

            dgv.ColumnHeadersHeight = 44;
            dgv.RowTemplate.Height = 48;

            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(252, 252, 252),
                ForeColor = Foreground,
                SelectionBackColor = Color.FromArgb(239, 246, 255),
                SelectionForeColor = Foreground
            };
        }

        // ===== Helper: Tạo page header (title + subtitle) =====
        public static Panel CreatePageHeader(string title, string subtitle, Control[]? actionButtons = null)
        {
            var header = new Panel
            {
                Dock = DockStyle.Top, Height = 80, BackColor = Background,
                Padding = new Padding(0, 8, 0, 8)
            };

            var lblTitle = new Label
            {
                Text = title, ForeColor = Foreground,
                Font = FontPageTitle, AutoSize = true,
                Location = new Point(0, 8)
            };
            header.Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text = subtitle, ForeColor = MutedForeground,
                Font = FontSubtitle, AutoSize = true,
                Location = new Point(0, 40)
            };
            header.Controls.Add(lblSub);

            if (actionButtons != null)
            {
                int rightX = 0;
                header.Resize += (s, e) =>
                {
                    rightX = header.Width;
                    for (int i = actionButtons.Length - 1; i >= 0; i--)
                    {
                        rightX -= actionButtons[i].Width + 12;
                        actionButtons[i].Location = new Point(rightX, 20);
                    }
                };
                foreach (var btn in actionButtons)
                {
                    btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                    header.Controls.Add(btn);
                }
            }

            return header;
        }

        // ===== Helper: Tạo search box =====
        public static TextBox CreateSearchBox(string placeholder = "Tìm kiếm...", int width = 280)
        {
            return new TextBox
            {
                Size = new Size(width, 36), PlaceholderText = placeholder,
                BackColor = Background, ForeColor = Foreground,
                BorderStyle = BorderStyle.FixedSingle,
                Font = FontBody
            };
        }

        // ===== Helper: Tạo filter combo =====
        public static ComboBox CreateFilterCombo(int width = 180)
        {
            return new ComboBox
            {
                Size = new Size(width, 36), DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Background, ForeColor = Foreground,
                FlatStyle = FlatStyle.Flat, Font = FontBody
            };
        }

        // ===== Helper: Tạo metric card =====
        public static Panel CreateMetricCard(string label, string value, string badge, Color badgeColor)
        {
            var card = new Panel
            {
                Size = new Size(260, 100), BackColor = Card,
                Padding = new Padding(20, 16, 20, 16)
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var pen = new Pen(Border, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            card.Controls.Add(new Label
            {
                Text = label, ForeColor = MutedForeground,
                Font = FontMetricLabel, AutoSize = true,
                Location = new Point(20, 16)
            });

            card.Controls.Add(new Label
            {
                Text = value, ForeColor = Foreground,
                Font = FontMetricValue, AutoSize = true,
                Location = new Point(20, 42)
            });

            if (!string.IsNullOrEmpty(badge))
            {
                var lblBadge = new Label
                {
                    Text = badge, ForeColor = badgeColor,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    AutoSize = true, BackColor = Color.FromArgb(40, badgeColor),
                    Padding = new Padding(6, 2, 6, 2)
                };
                lblBadge.Location = new Point(card.Width - lblBadge.PreferredWidth - 20, 42);
                card.Controls.Add(lblBadge);
            }

            return card;
        }

        // ===== Helper: Tạo status badge =====
        public static Label CreateBadge(string text, Color fg, Color bg)
        {
            return new Label
            {
                Text = text, ForeColor = fg, BackColor = bg,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                AutoSize = true, Padding = new Padding(8, 3, 8, 3),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }
    }
}
