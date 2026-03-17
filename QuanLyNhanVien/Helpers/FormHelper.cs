using QuanLyNhanVien.Models.Enums;

namespace QuanLyNhanVien.Helpers
{
    /// <summary>
    /// Helper cho form: phân quyền buttons, embed form, dialog chuẩn
    /// </summary>
    public static class FormHelper
    {
        /// <summary>
        /// Auto enable/disable buttons theo quyền RBAC.
        /// Quy ước: button có Tag = "Add", "Edit", "Delete", "Export", "Print"
        /// </summary>
        public static void ApplyPermissions(Control container, string menuCode)
        {
            foreach (Control ctrl in container.Controls)
            {
                if (ctrl is Button btn && btn.Tag is string tag)
                {
                    btn.Enabled = tag switch
                    {
                        "Add" => AppSession.HasPermission(menuCode, PermissionType.Add),
                        "Edit" => AppSession.HasPermission(menuCode, PermissionType.Edit),
                        "Delete" => AppSession.HasPermission(menuCode, PermissionType.Delete),
                        "Export" => AppSession.HasPermission(menuCode, PermissionType.Export),
                        "Print" => AppSession.HasPermission(menuCode, PermissionType.Print),
                        _ => btn.Enabled
                    };
                }

                // Recursive cho nested panels/groups
                if (ctrl.HasChildren)
                    ApplyPermissions(ctrl, menuCode);
            }
        }

        /// <summary>
        /// Nhúng form con vào panel content của FrmMain
        /// </summary>
        public static void EmbedForm(Panel container, Form childForm)
        {
            container.Controls.Clear();
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            childForm.BackColor = container.BackColor;
            container.Controls.Add(childForm);
            childForm.Show();
        }

        /// <summary>
        /// Dialog xác nhận xóa chuẩn
        /// </summary>
        public static bool ConfirmDelete(string itemName = "mục này")
        {
            return MessageBox.Show(
                $"Bạn có chắc muốn xóa {itemName}?\nThao tác này không thể hoàn tác.",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        /// <summary>
        /// Hiển thị thông báo thành công
        /// </summary>
        public static void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Hiển thị thông báo lỗi
        /// </summary>
        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
