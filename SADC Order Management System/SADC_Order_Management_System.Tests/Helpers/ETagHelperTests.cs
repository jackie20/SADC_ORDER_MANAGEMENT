using FluentAssertions;
using SADC_Order_Management_System.Helpers;
using Xunit;

namespace SADC_Order_Management_System.SADC_Order_Management_System.Tests.Helpers
{
    public class ETagHelperTests
    {
        [Fact]
        public void FromRowVersion_Should_Return_Base64_String()
        {
            var bytes = new byte[] { 1, 2, 3, 4 };

            var result = ETagHelper.FromRowVersion(bytes);

            result.Should().Be(Convert.ToBase64String(bytes));
        }
    }
}
