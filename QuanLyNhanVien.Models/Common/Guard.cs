namespace QuanLyNhanVien.Models.Common
{
    /// <summary>
    /// ARCH-3/5: Guard clauses — validation helper cho arguments
    /// Giúp giảm boilerplate null/empty checks, làm code sạch hơn
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// Kiểm tra string không rỗng
        /// </summary>
        public static string NotNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{paramName} không được để trống.", paramName);
            return value;
        }

        /// <summary>
        /// ID phải > 0
        /// </summary>
        public static int PositiveId(int value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentException($"{paramName} phải lớn hơn 0.", paramName);
            return value;
        }

        /// <summary>
        /// Số phải >= 0
        /// </summary>
        public static decimal NonNegative(decimal value, string paramName)
        {
            if (value < 0)
                throw new ArgumentException($"{paramName} không được âm.", paramName);
            return value;
        }

        /// <summary>
        /// Ngày kết thúc phải >= ngày bắt đầu
        /// </summary>
        public static (DateTime Start, DateTime End) ValidDateRange(DateTime start, DateTime end, string paramName = "Khoảng thời gian")
        {
            if (end < start)
                throw new ArgumentException($"{paramName}: Ngày kết thúc phải >= ngày bắt đầu.", paramName);
            return (start, end);
        }

        /// <summary>
        /// Object không null
        /// </summary>
        public static T NotNull<T>(T? value, string paramName) where T : class
        {
            return value ?? throw new ArgumentNullException(paramName, $"{paramName} không được null.");
        }

        /// <summary>
        /// Tháng 1-12
        /// </summary>
        public static int ValidMonth(int month)
        {
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "Tháng phải từ 1-12.");
            return month;
        }

        /// <summary>
        /// Năm hợp lệ (>= 2000)
        /// </summary>
        public static int ValidYear(int year)
        {
            if (year < 2000 || year > 2100)
                throw new ArgumentOutOfRangeException(nameof(year), "Năm phải từ 2000-2100.");
            return year;
        }
    }
}
