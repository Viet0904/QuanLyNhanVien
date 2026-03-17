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
    }
}
