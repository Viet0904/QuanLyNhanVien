using QuanLyNhanVien.BLL.Services;

namespace QuanLyNhanVien.Tests
{
    /// <summary>
    /// Unit tests cho SalaryService.CalculatePIT() — Thuế TNCN lũy tiến 7 bậc
    /// </summary>
    public class SalaryServiceTests
    {
        [Fact]
        public void CalculatePIT_ZeroIncome_ReturnsZero()
        {
            Assert.Equal(0, SalaryService.CalculatePIT(0));
        }

        [Fact]
        public void CalculatePIT_NegativeIncome_ReturnsZero()
        {
            Assert.Equal(0, SalaryService.CalculatePIT(-5_000_000m));
        }

        [Theory]
        [InlineData(1_000_000, 50_000)]           // 1M * 5% = 50K
        [InlineData(5_000_000, 250_000)]           // 5M * 5% = 250K (hết bậc 1)
        public void CalculatePIT_Bracket1_5Percent(decimal income, decimal expectedTax)
        {
            Assert.Equal(expectedTax, SalaryService.CalculatePIT(income));
        }

        [Theory]
        [InlineData(10_000_000, 750_000)]          // 5M*5% + 5M*10% = 250K + 500K = 750K
        [InlineData(7_000_000, 450_000)]           // 5M*5% + 2M*10% = 250K + 200K = 450K
        public void CalculatePIT_Bracket2_10Percent(decimal income, decimal expectedTax)
        {
            Assert.Equal(expectedTax, SalaryService.CalculatePIT(income));
        }

        [Theory]
        [InlineData(18_000_000, 1_950_000)]        // 250K + 500K + 8M*15% = 250K + 500K + 1200K = 1950K
        [InlineData(32_000_000, 4_750_000)]        // 250K + 500K + 1200K + 14M*20% = 4750K
        [InlineData(52_000_000, 9_750_000)]        // 4750K + 20M*25% = 9750K
        [InlineData(80_000_000, 18_150_000)]       // 9750K + 28M*30% = 18150K
        public void CalculatePIT_HigherBrackets(decimal income, decimal expectedTax)
        {
            Assert.Equal(expectedTax, SalaryService.CalculatePIT(income));
        }

        [Fact]
        public void CalculatePIT_Bracket7_35Percent()
        {
            // 100M = 80M brackets + 20M*35% = 18150K + 7000K = 25150K
            var tax = SalaryService.CalculatePIT(100_000_000m);
            Assert.Equal(25_150_000m, tax);
        }

        [Fact]
        public void CalculatePIT_SmallAmount_CorrectRounding()
        {
            // 3.5M * 5% = 175K (exact)
            Assert.Equal(175_000m, SalaryService.CalculatePIT(3_500_000m));
        }
    }
}
