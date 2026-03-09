using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SADC_Order_Management_System.Controllers;
using SADC_Order_Management_System.DTOs.Responses;
using SADC_Order_Management_System.Services.Interfaces;
using Xunit;

namespace SADC_Order_Management_System.Tests.Controllers
{
    public class ReportsControllerTests
    {
        private readonly Mock<IFxService> _fxService = new();
        private readonly ReportsController _controller;

        public ReportsControllerTests()
        {
            _controller = new ReportsController(_fxService.Object);
        }

        [Fact]
        public async Task GetOrdersInZar_Should_Return_Ok()
        {
            _fxService.Setup(x => x.GetOrdersZarReportAsync())
                .ReturnsAsync(new OrdersZarReportResponseDto());

            var result = await _controller.GetOrdersInZar();

            result.Result.Should().BeOfType<OkObjectResult>();
        }
    }
}