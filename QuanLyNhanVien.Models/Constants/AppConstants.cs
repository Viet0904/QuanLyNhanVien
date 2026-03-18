namespace QuanLyNhanVien.Models.Constants
{
    public static class AppConstants
    {
        public const string AppName = "Quản Lý Nhân Viên";
        public const string AppVersion = "1.0.0";

        // Pagination
        public const int DefaultPageSize = 50;
        public const int MaxPageSize = 500;

        // Password policy
        public const int MinPasswordLength = 6;
        public const int MaxLoginAttempts = 5;
        public const int LockoutMinutes = 15;
        public const int BcryptWorkFactor = 12;

        // JSON Cache
        public const string CategoryCacheFile = "Data\\categories.json";
        public const int CacheExpiryMinutes = 60;

        // Auto-lock
        public const int AutoLockMinutes = 30;

        // Employee code format
        public const string EmployeeCodePrefix = "NV";
        public const int EmployeeCodeLength = 4; // NV-0001

        // Salary defaults (VN)
        public const decimal DefaultPersonalDeduction = 11_000_000m;
        public const decimal DefaultDependentDeduction = 4_400_000m;
        public const decimal DefaultStandardWorkDays = 26m;

        // Insurance rates (employee portion)
        public const decimal SocialInsuranceRate = 0.08m;      // 8%
        public const decimal HealthInsuranceRate = 0.015m;     // 1.5%
        public const decimal UnemploymentInsuranceRate = 0.01m; // 1%

        /// <summary>
        /// Kiểm tra độ mạnh mật khẩu: ít nhất 6 ký tự, có chữ hoa, chữ thường, số
        /// </summary>
        public static (bool IsValid, string Message) ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Mật khẩu không được để trống.");
            if (password.Length < MinPasswordLength)
                return (false, $"Mật khẩu phải có ít nhất {MinPasswordLength} ký tự.");
            if (!password.Any(char.IsUpper))
                return (false, "Mật khẩu phải chứa ít nhất 1 chữ hoa (A-Z).");
            if (!password.Any(char.IsLower))
                return (false, "Mật khẩu phải chứa ít nhất 1 chữ thường (a-z).");
            if (!password.Any(char.IsDigit))
                return (false, "Mật khẩu phải chứa ít nhất 1 chữ số (0-9).");
            return (true, "");
        }
    }
}
