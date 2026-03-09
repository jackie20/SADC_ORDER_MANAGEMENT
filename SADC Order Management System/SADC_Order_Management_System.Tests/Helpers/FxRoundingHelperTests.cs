using FluentAssertions;
using SADC_Order_Management_System.Helpers;
using Xunit;

namespace SADC_Order_Management_System.SADC_Order_Management_System.Tests.Helpers
{
    public class FxRoundingHelperTests
    {
        [Theory]
        [InlineData(10.125, 10.12)]
        [InlineData(10.135, 10.14)]
        [InlineData(99.999, 100.00)]
        public void RoundMoney_Should_Use_BankersRounding(decimal input, decimal expected)
        {
            var result = FxRoundingHelper.RoundMoney(input);
            result.Should().Be(expected);
        }
    }
}
