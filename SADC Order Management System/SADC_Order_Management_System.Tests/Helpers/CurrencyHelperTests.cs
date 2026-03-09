using SADC_Order_Management_System.Helpers;
using FluentAssertions;
using SADC_Order_Management_System.Helpers;
using Xunit;

namespace SADC_Order_Management_System.SADC_Order_Management_System.Tests.Helpers
{
    public class CurrencyHelperTests
    {
        [Theory]
        [InlineData("ZA", "ZAR", true)]
        [InlineData("BW", "BWP", true)]
        [InlineData("ZW", "USD", true)]
        [InlineData("ZW", "ZWL", true)]
        [InlineData("NA", "ZAR", true)]
        [InlineData("ZA", "USD", false)]
        [InlineData("BW", "ZAR", false)]
        [InlineData("", "ZAR", false)]
        [InlineData("ZA", "", false)]
        public void IsValidCountryCurrencyPair_Should_Return_Expected_Result(
            string country,
            string currency,
            bool expected)
        {
            var result = CurrencyHelper.IsValidCountryCurrencyPair(country, currency);
            result.Should().Be(expected);
        }
    }
}
