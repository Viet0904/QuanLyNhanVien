using QuanLyNhanVien.Helpers;
using QuanLyNhanVien.Models.Entities;

namespace QuanLyNhanVien.Forms.Category
{
    /// <summary>
    /// Quản lý danh mục: bên trái = Category list, bên phải = CategoryItems
    /// Buttons nằm trong header bar thay vì toolbar riêng (tránh docking issues khi embed)
    /// </summary>
    public class FrmCategoryManager : Form
    {
        private DataGridView dgvCategories = null!, dgvItems = null!;
        private Button btnAddCat = null!, btnEditCat = null!, btnDelCat = null!, btnRefresh = null!;
        private Button btnAddItem = null!, btnEditItem = null!, btnDelItem = null!;
        private Label lblItemTitle = null!;
        private Models.Entities.Category? _selectedCat;
        private readonly string _menuCode = "HT_DANHMUC";

        public FrmCategoryManager()
        {
            InitializeComponent();
            this.Load += async (s, e) =>
            {
                FormHelper.ApplyPermissions(this, _menuCode);
                await LoadCategoriesAsync();
            };
        }

        private void InitializeComponent()
        {
            this.Text = "Quản Lý Danh Mục";
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = Color.FromArgb(30, 30, 46);

            // ===== MAIN: SplitContainer =====
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 450,
                BackColor = Color.FromArgb(50, 50, 70),
                SplitterWidth = 4,
                BorderStyle = BorderStyle.None
            };

            // ————— Panel 1: Category list —————
            split.Panel1.BackColor = Color.FromArgb(35, 35, 55);

            // Header bar trái (title + buttons)
            var leftHeader = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.FromArgb(45, 45, 68), Padding = new Padding(8, 0, 0, 0) };
            leftHeader.Controls.Add(new Label
            {
                Text = "📂 Danh Mục", Dock = DockStyle.Left, Width = 130,
                ForeColor = Color.FromArgb(200, 210, 240), Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            });

            var leftBtnPanel = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 250, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 5, 5, 0) };
            btnRefresh = SmallBtn("🔄", "", Color.FromArgb(100, 100, 140), 35);
            btnDelCat = SmallBtn("🗑️", "Delete", Color.FromArgb(200, 60, 60), 35);
            btnEditCat = SmallBtn("✏️", "Edit", Color.FromArgb(87, 163, 75), 35);
            btnAddCat = SmallBtn("➕", "Add", Color.FromArgb(88, 101, 242), 35);
            leftBtnPanel.Controls.AddRange(new Control[] { btnRefresh, btnDelCat, btnEditCat, btnAddCat });
            leftHeader.Controls.Add(leftBtnPanel);

            btnAddCat.Click += BtnAddCat_Click;
            btnEditCat.Click += BtnEditCat_Click;
            btnDelCat.Click += BtnDelCat_Click;
            btnRefresh.Click += async (s, e) => await LoadCategoriesAsync(true);

            dgvCategories = CreateGrid();
            dgvCategories.SelectionChanged += DgvCategories_SelectionChanged;
            dgvCategories.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEditCat_Click(s, e); };

            // Docking order chuẩn: Fill trước, Top sau
            split.Panel1.Controls.Add(dgvCategories);   // Fill
            split.Panel1.Controls.Add(leftHeader);       // Top

            // ————— Panel 2: Category items —————
            split.Panel2.BackColor = Color.FromArgb(35, 35, 55);

            // Header bar phải (title + buttons)
            var rightHeader = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.FromArgb(45, 45, 68), Padding = new Padding(8, 0, 0, 0) };
            lblItemTitle = new Label
            {
                Text = "📋 Chọn danh mục bên trái", Dock = DockStyle.Left, Width = 280,
                ForeColor = Color.FromArgb(200, 210, 240), Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            rightHeader.Controls.Add(lblItemTitle);

            var rightBtnPanel = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 130, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 5, 5, 0) };
            btnDelItem = SmallBtn("🗑️", "Delete", Color.FromArgb(200, 60, 60), 35);
            btnEditItem = SmallBtn("✏️", "Edit", Color.FromArgb(87, 163, 75), 35);
            btnAddItem = SmallBtn("➕", "Add", Color.FromArgb(88, 101, 242), 35);
            rightBtnPanel.Controls.AddRange(new Control[] { btnDelItem, btnEditItem, btnAddItem });
            rightHeader.Controls.Add(rightBtnPanel);

            btnAddItem.Click += BtnAddItem_Click;
            btnEditItem.Click += BtnEditItem_Click;
            btnDelItem.Click += BtnDelItem_Click;

            dgvItems = CreateGrid();
            dgvItems.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEditItem_Click(s, e); };

            // Docking order chuẩn: Fill trước, Top sau
            split.Panel2.Controls.Add(dgvItems);         // Fill
            split.Panel2.Controls.Add(rightHeader);       // Top

            // Form chỉ có 1 control duy nhất = SplitContainer (Dock=Fill)
            this.Controls.Add(split);
        }

        private DataGridView CreateGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.FromArgb(35, 35, 55), GridColor = Color.FromArgb(60, 60, 80),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(40, 40, 60), ForeColor = Color.FromArgb(200, 210, 230),
                    SelectionBackColor = Color.FromArgb(88, 101, 242), Font = new Font("Segoe UI", 10F)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(50, 50, 75), ForeColor = Color.FromArgb(180, 190, 220),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                },
                EnableHeadersVisualStyles = false, RowHeadersVisible = false, BorderStyle = BorderStyle.None
            };
        }

        private Button SmallBtn(string text, string tag, Color c, int w)
        {
            var b = new Button
            {
                Text = text, Tag = tag, Size = new Size(w, 30), FlatStyle = FlatStyle.Flat,
                BackColor = c, ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(3, 0, 0, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        // ===== Data Loading =====
        private async Task LoadCategoriesAsync(bool forceRefresh = false)
        {
            var cats = (await Program.CatService.GetAllAsync(forceRefresh)).ToList();
            dgvCategories.DataSource = null;
            dgvCategories.DataSource = cats;

            HideCol(dgvCategories, "CategoryId", "Items");
            SetHeader(dgvCategories, "CategoryCode", "Mã");
            SetHeader(dgvCategories, "CategoryName", "Tên Danh Mục");
            SetHeader(dgvCategories, "Description", "Mô Tả");
            SetHeader(dgvCategories, "IsSystem", "HT");

            // Cố định width cho cột "Mã" và "HT"
            if (dgvCategories.Columns.Contains("CategoryCode"))
            {
                dgvCategories.Columns["CategoryCode"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvCategories.Columns["CategoryCode"].Width = 100;
            }
            if (dgvCategories.Columns.Contains("IsSystem"))
            {
                dgvCategories.Columns["IsSystem"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgvCategories.Columns["IsSystem"].Width = 40;
            }

            if (forceRefresh)
                FormHelper.ShowSuccess("Đã làm mới cache danh mục!");
        }

        private void DgvCategories_SelectionChanged(object? s, EventArgs e)
        {
            _selectedCat = dgvCategories.CurrentRow?.DataBoundItem as Models.Entities.Category;
            if (_selectedCat != null)
            {
                lblItemTitle.Text = $"📋 {_selectedCat.CategoryName}";
                dgvItems.DataSource = null;
                dgvItems.DataSource = _selectedCat.Items.Where(i => i.IsActive).ToList();

                HideCol(dgvItems, "ItemId", "CategoryId", "IsActive");
                SetHeader(dgvItems, "ItemCode", "Mã");
                SetHeader(dgvItems, "ItemName", "Tên");
                SetHeader(dgvItems, "ItemValue", "Giá Trị");
                SetHeader(dgvItems, "SortOrder", "TT");

                if (dgvItems.Columns.Contains("SortOrder"))
                {
                    dgvItems.Columns["SortOrder"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvItems.Columns["SortOrder"].Width = 50;
                }
                if (dgvItems.Columns.Contains("ItemCode"))
                {
                    dgvItems.Columns["ItemCode"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvItems.Columns["ItemCode"].Width = 100;
                }
            }
        }

        private void HideCol(DataGridView g, params string[] cols) { foreach (var c in cols) if (g.Columns.Contains(c)) g.Columns[c].Visible = false; }
        private void SetHeader(DataGridView g, string col, string h) { if (g.Columns.Contains(col)) g.Columns[col].HeaderText = h; }

        // ===== Category CRUD =====
        private async void BtnAddCat_Click(object? s, EventArgs e)
        {
            var dlg = new FrmCategoryDetail(null);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadCategoriesAsync(true);
        }
        private async void BtnEditCat_Click(object? s, EventArgs e)
        {
            if (_selectedCat == null) return;
            if (_selectedCat.IsSystem) { FormHelper.ShowError("Không thể sửa danh mục hệ thống."); return; }
            var dlg = new FrmCategoryDetail(_selectedCat);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadCategoriesAsync(true);
        }
        private async void BtnDelCat_Click(object? s, EventArgs e)
        {
            if (_selectedCat == null) return;
            if (_selectedCat.IsSystem) { FormHelper.ShowError("Không thể xóa danh mục hệ thống."); return; }
            if (!FormHelper.ConfirmDelete(_selectedCat.CategoryName)) return;
            var (ok, msg) = await Program.CatService.DeleteCategoryAsync(_selectedCat.CategoryId);
            if (ok) { FormHelper.ShowSuccess(msg); await LoadCategoriesAsync(true); } else FormHelper.ShowError(msg);
        }

        // ===== Item CRUD =====
        private async void BtnAddItem_Click(object? s, EventArgs e)
        {
            if (_selectedCat == null) { FormHelper.ShowError("Vui lòng chọn danh mục trước."); return; }
            var dlg = new FrmItemDetail(null, _selectedCat.CategoryId);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadCategoriesAsync(true);
        }
        private async void BtnEditItem_Click(object? s, EventArgs e)
        {
            var item = dgvItems.CurrentRow?.DataBoundItem as CategoryItem;
            if (item == null) return;
            var dlg = new FrmItemDetail(item, _selectedCat?.CategoryId ?? 0);
            if (dlg.ShowDialog() == DialogResult.OK) await LoadCategoriesAsync(true);
        }
        private async void BtnDelItem_Click(object? s, EventArgs e)
        {
            var item = dgvItems.CurrentRow?.DataBoundItem as CategoryItem;
            if (item == null) return;
            if (!FormHelper.ConfirmDelete(item.ItemName)) return;
            await Program.CatService.DeleteItemAsync(item.ItemId);
            await LoadCategoriesAsync(true);
        }
    }

    // === Category Detail Dialog ===
    public class FrmCategoryDetail : Form
    {
        private TextBox txtCode = null!, txtName = null!, txtDesc = null!;
        private Models.Entities.Category? _cat;

        public FrmCategoryDetail(Models.Entities.Category? cat)
        {
            _cat = cat;
            this.Text = _cat == null ? "Thêm Danh Mục" : "Sửa Danh Mục";
            this.Size = new Size(420, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(40, 40, 60);
            this.Font = new Font("Segoe UI", 10F);

            var y = 15;
            Lbl("Mã danh mục:", 20, y); txtCode = Txt(150, y, 220); y += 40;
            Lbl("Tên danh mục:", 20, y); txtName = Txt(150, y, 220); y += 40;
            Lbl("Mô tả:", 20, y); txtDesc = Txt(150, y, 220); txtDesc.Multiline = true; txtDesc.Height = 50; y += 65;

            var btnSave = new Button { Text = "💾 Lưu", Location = new Point(150, y), Size = new Size(110, 38), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(88, 101, 242), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            if (_cat != null) { txtCode.Text = _cat.CategoryCode; txtName.Text = _cat.CategoryName; txtDesc.Text = _cat.Description; }
        }

        private void Lbl(string t, int x, int y) { this.Controls.Add(new Label { Text = t, Location = new Point(x, y + 3), AutoSize = true, ForeColor = Color.FromArgb(180, 190, 220) }); }
        private TextBox Txt(int x, int y, int w)
        {
            var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 28), BackColor = Color.FromArgb(55, 55, 80), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(tb); return tb;
        }

        private async void BtnSave_Click(object? s, EventArgs e)
        {
            if (_cat == null)
            {
                var cat = new Models.Entities.Category { CategoryCode = txtCode.Text.Trim(), CategoryName = txtName.Text.Trim(), Description = txtDesc.Text.Trim() };
                var (ok, msg, _) = await Program.CatService.CreateCategoryAsync(cat);
                if (ok) { this.DialogResult = DialogResult.OK; this.Close(); } else FormHelper.ShowError(msg);
            }
            else
            {
                _cat.CategoryCode = txtCode.Text.Trim(); _cat.CategoryName = txtName.Text.Trim(); _cat.Description = txtDesc.Text.Trim();
                var (ok, msg) = await Program.CatService.UpdateCategoryAsync(_cat);
                if (ok) { this.DialogResult = DialogResult.OK; this.Close(); } else FormHelper.ShowError(msg);
            }
        }
    }

    // === Item Detail Dialog ===
    public class FrmItemDetail : Form
    {
        private TextBox txtCode = null!, txtName = null!, txtValue = null!;
        private NumericUpDown nudOrder = null!;
        private CategoryItem? _item;
        private int _catId;

        public FrmItemDetail(CategoryItem? item, int catId)
        {
            _item = item; _catId = catId;
            this.Text = _item == null ? "Thêm Mục" : "Sửa Mục";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(40, 40, 60);
            this.Font = new Font("Segoe UI", 10F);

            var y = 15;
            Lbl("Mã:", 20, y); txtCode = Txt(120, y, 230); y += 38;
            Lbl("Tên:", 20, y); txtName = Txt(120, y, 230); y += 38;
            Lbl("Giá trị:", 20, y); txtValue = Txt(120, y, 230); y += 38;
            Lbl("Thứ tự:", 20, y);
            nudOrder = new NumericUpDown { Location = new Point(120, y), Size = new Size(80, 28), BackColor = Color.FromArgb(55, 55, 80), ForeColor = Color.White, Minimum = 0, Maximum = 100 };
            this.Controls.Add(nudOrder); y += 48;

            var btnSave = new Button { Text = "💾 Lưu", Location = new Point(120, y), Size = new Size(110, 38), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(88, 101, 242), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            if (_item != null) { txtCode.Text = _item.ItemCode; txtName.Text = _item.ItemName; txtValue.Text = _item.ItemValue; nudOrder.Value = _item.SortOrder; }
        }

        private void Lbl(string t, int x, int y) { this.Controls.Add(new Label { Text = t, Location = new Point(x, y + 3), AutoSize = true, ForeColor = Color.FromArgb(180, 190, 220) }); }
        private TextBox Txt(int x, int y, int w)
        {
            var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 28), BackColor = Color.FromArgb(55, 55, 80), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(tb); return tb;
        }

        private async void BtnSave_Click(object? s, EventArgs e)
        {
            if (_item == null)
            {
                var item = new CategoryItem { CategoryId = _catId, ItemCode = txtCode.Text.Trim(), ItemName = txtName.Text.Trim(), ItemValue = txtValue.Text.Trim(), SortOrder = (int)nudOrder.Value };
                var (ok, msg, _) = await Program.CatService.CreateItemAsync(item);
                if (ok) { this.DialogResult = DialogResult.OK; this.Close(); } else FormHelper.ShowError(msg);
            }
            else
            {
                _item.ItemCode = txtCode.Text.Trim(); _item.ItemName = txtName.Text.Trim(); _item.ItemValue = txtValue.Text.Trim(); _item.SortOrder = (int)nudOrder.Value;
                var (ok, msg) = await Program.CatService.UpdateItemAsync(_item);
                if (ok) { this.DialogResult = DialogResult.OK; this.Close(); } else FormHelper.ShowError(msg);
            }
        }
    }
}
