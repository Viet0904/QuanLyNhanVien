using Microsoft.Extensions.Configuration;
using QuanLyNhanVien.BLL.Cache;
using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Forms.Main;

namespace QuanLyNhanVien
{
    internal static class Program
    {
        // Service instances (Poor man's DI cho WinForms)
        public static DbConnectionFactory DbFactory { get; private set; } = null!;
        public static AuthService AuthService { get; private set; } = null!;
        public static EmployeeService EmployeeService { get; private set; } = null!;
        public static DepartmentService DeptService { get; private set; } = null!;
        public static PositionService PosService { get; private set; } = null!;
        public static MenuService MenuService { get; private set; } = null!;
        public static CategoryService CatService { get; private set; } = null!;
        public static JsonCacheManager CacheManager { get; private set; } = null!;

        // Repositories (cho các form cần truy cập trực tiếp)
        public static UserRepository UserRepo { get; private set; } = null!;
        public static EmployeeRepository EmployeeRepo { get; private set; } = null!;
        public static DepartmentRepository DeptRepo { get; private set; } = null!;
        public static PositionRepository PosRepo { get; private set; } = null!;
        public static MenuRepository MenuRepo { get; private set; } = null!;
        public static RoleRepository RoleRepo { get; private set; } = null!;
        public static CategoryRepository CategoryRepo { get; private set; } = null!;

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            // Load configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connStr = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Initialize services
            InitializeServices(connStr);

            // Show login form
            var loginForm = new FrmLogin();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new FrmMain());
            }
        }

        private static void InitializeServices(string connectionString)
        {
            DbFactory = new DbConnectionFactory(connectionString);

            // Repositories
            UserRepo = new UserRepository(DbFactory);
            EmployeeRepo = new EmployeeRepository(DbFactory);
            DeptRepo = new DepartmentRepository(DbFactory);
            PosRepo = new PositionRepository(DbFactory);
            MenuRepo = new MenuRepository(DbFactory);
            RoleRepo = new RoleRepository(DbFactory);
            CategoryRepo = new CategoryRepository(DbFactory);

            // Services
            AuthService = new AuthService(UserRepo);
            EmployeeService = new EmployeeService(EmployeeRepo, DeptRepo, PosRepo);
            DeptService = new DepartmentService(DeptRepo);
            PosService = new PositionService(PosRepo);
            MenuService = new MenuService(MenuRepo, RoleRepo);
            CacheManager = new JsonCacheManager(CategoryRepo, AppDomain.CurrentDomain.BaseDirectory);
            CatService = new CategoryService(CategoryRepo, CacheManager);
        }
    }
}