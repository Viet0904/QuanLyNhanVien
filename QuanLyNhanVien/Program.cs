using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuanLyNhanVien.BLL.Cache;
using QuanLyNhanVien.BLL.Services;
using QuanLyNhanVien.DAL.Context;
using QuanLyNhanVien.DAL.Repositories;
using QuanLyNhanVien.Forms.Main;

namespace QuanLyNhanVien
{
    internal static class Program
    {
        /// <summary>
        /// DI Service Provider — nguồn duy nhất để resolve services.
        /// </summary>
        public static IServiceProvider Services { get; private set; } = null!;

        // Backward-compatible accessors (delegate sang DI container)
        public static DbConnectionFactory DbFactory => Services.GetRequiredService<DbConnectionFactory>();
        public static AuthService AuthService => Services.GetRequiredService<AuthService>();
        public static EmployeeService EmployeeService => Services.GetRequiredService<EmployeeService>();
        public static DepartmentService DeptService => Services.GetRequiredService<DepartmentService>();
        public static PositionService PosService => Services.GetRequiredService<PositionService>();
        public static MenuService MenuService => Services.GetRequiredService<MenuService>();
        public static CategoryService CatService => Services.GetRequiredService<CategoryService>();
        public static JsonCacheManager CacheManager => Services.GetRequiredService<JsonCacheManager>();
        public static AttendanceService AttendanceService => Services.GetRequiredService<AttendanceService>();
        public static LeaveService LeaveService => Services.GetRequiredService<LeaveService>();
        public static SalaryService SalaryService => Services.GetRequiredService<SalaryService>();
        public static ReportService ReportService => Services.GetRequiredService<ReportService>();
        public static UserService UserService => Services.GetRequiredService<UserService>();
        public static BackupService BackupService => Services.GetRequiredService<BackupService>();
        public static CompanySettingsService CompanySettingsService => Services.GetRequiredService<CompanySettingsService>();
        public static ContractService ContractService => Services.GetRequiredService<ContractService>();
        public static EmployeeEventService EmployeeEventService => Services.GetRequiredService<EmployeeEventService>();
        public static AdvanceService AdvanceService => Services.GetRequiredService<AdvanceService>();
        public static AuditService AuditService => Services.GetRequiredService<AuditService>();
        public static EmailService EmailService => Services.GetRequiredService<EmailService>();

        // Repositories (backward-compatible)
        public static UserRepository UserRepo => Services.GetRequiredService<UserRepository>();
        public static EmployeeRepository EmployeeRepo => Services.GetRequiredService<EmployeeRepository>();
        public static DepartmentRepository DeptRepo => Services.GetRequiredService<DepartmentRepository>();
        public static PositionRepository PosRepo => Services.GetRequiredService<PositionRepository>();
        public static MenuRepository MenuRepo => Services.GetRequiredService<MenuRepository>();
        public static RoleRepository RoleRepo => Services.GetRequiredService<RoleRepository>();
        public static CategoryRepository CategoryRepo => Services.GetRequiredService<CategoryRepository>();
        public static AttendanceRepository AttendanceRepo => Services.GetRequiredService<AttendanceRepository>();
        public static LeaveRequestRepository LeaveRepo => Services.GetRequiredService<LeaveRequestRepository>();
        public static SalaryRepository SalaryRepo => Services.GetRequiredService<SalaryRepository>();
        public static ReportRepository ReportRepo => Services.GetRequiredService<ReportRepository>();
        public static AuditLogRepository AuditLogRepo => Services.GetRequiredService<AuditLogRepository>();
        public static CompanySettingsRepository CompanySettingsRepo => Services.GetRequiredService<CompanySettingsRepository>();
        public static ContractRepository ContractRepo => Services.GetRequiredService<ContractRepository>();
        public static EmployeeEventRepository EmployeeEventRepo => Services.GetRequiredService<EmployeeEventRepository>();
        public static AdvanceRepository AdvanceRepo => Services.GetRequiredService<AdvanceRepository>();
        public static HolidayRepository HolidayRepo => Services.GetRequiredService<HolidayRepository>();

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

            // Build DI container
            Services = ConfigureServices(connStr);

            // Show login form
            var loginForm = new FrmLogin();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new FrmMain());
            }
        }

        private static IServiceProvider ConfigureServices(string connectionString)
        {
            var services = new ServiceCollection();

            // Infrastructure
            services.AddSingleton(new DbConnectionFactory(connectionString));

            // Repositories (Singleton — stateless, thread-safe với Dapper)
            services.AddSingleton<UserRepository>();
            services.AddSingleton<EmployeeRepository>();
            services.AddSingleton<DepartmentRepository>();
            services.AddSingleton<PositionRepository>();
            services.AddSingleton<MenuRepository>();
            services.AddSingleton<RoleRepository>();
            services.AddSingleton<CategoryRepository>();
            services.AddSingleton<AttendanceRepository>();
            services.AddSingleton<LeaveRequestRepository>();
            services.AddSingleton<SalaryRepository>();
            services.AddSingleton<ReportRepository>();
            services.AddSingleton<AuditLogRepository>();
            services.AddSingleton<CompanySettingsRepository>();
            services.AddSingleton<ContractRepository>();
            services.AddSingleton<EmployeeEventRepository>();
            services.AddSingleton<AdvanceRepository>();
            services.AddSingleton<HolidayRepository>();

            // Cache
            services.AddSingleton(sp => new JsonCacheManager(
                sp.GetRequiredService<CategoryRepository>(),
                AppDomain.CurrentDomain.BaseDirectory));

            // Services
            services.AddSingleton<AuthService>();
            services.AddSingleton(sp => new EmployeeService(
                sp.GetRequiredService<EmployeeRepository>(),
                sp.GetRequiredService<DepartmentRepository>(),
                sp.GetRequiredService<PositionRepository>()));
            services.AddSingleton<DepartmentService>();
            services.AddSingleton<PositionService>();
            services.AddSingleton(sp => new MenuService(
                sp.GetRequiredService<MenuRepository>(),
                sp.GetRequiredService<RoleRepository>()));
            services.AddSingleton(sp => new CategoryService(
                sp.GetRequiredService<CategoryRepository>(),
                sp.GetRequiredService<JsonCacheManager>()));
            services.AddSingleton(sp => new AttendanceService(
                sp.GetRequiredService<AttendanceRepository>(),
                sp.GetRequiredService<EmployeeRepository>()));
            services.AddSingleton(sp => new LeaveService(
                sp.GetRequiredService<LeaveRequestRepository>(),
                sp.GetRequiredService<HolidayRepository>(),
                sp.GetRequiredService<AttendanceRepository>()));
            services.AddSingleton(sp => new SalaryService(
                sp.GetRequiredService<SalaryRepository>(),
                sp.GetRequiredService<AttendanceRepository>(),
                sp.GetRequiredService<EmployeeRepository>(),
                sp.GetRequiredService<PositionRepository>(),
                sp.GetRequiredService<AdvanceRepository>()));
            services.AddSingleton<ReportService>();
            services.AddSingleton(sp => new UserService(
                sp.GetRequiredService<UserRepository>(),
                sp.GetRequiredService<RoleRepository>(),
                sp.GetRequiredService<AuditService>()));
            services.AddSingleton(sp => new BackupService(
                sp.GetRequiredService<DbConnectionFactory>(),
                sp.GetRequiredService<AuditService>()));
            services.AddSingleton<CompanySettingsService>();
            services.AddSingleton(sp => new ContractService(
                sp.GetRequiredService<ContractRepository>(),
                sp.GetRequiredService<EmployeeRepository>()));
            services.AddSingleton<EmployeeEventService>();
            services.AddSingleton<AdvanceService>();
            services.AddSingleton<AuditService>();
            services.AddSingleton<EmailService>();

            return services.BuildServiceProvider();
        }
    }
}