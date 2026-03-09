using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SADC_Order_Management_System.Controllers;
using SADC_Order_Management_System.DTOs.Requests;
using SADC_Order_Management_System.DTOs.Responses;
using SADC_Order_Management_System.Helpers;
using SADC_Order_Management_System.Services.Interfaces;
using Xunit;

namespace SADC_Order_Management_System.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _service = new();
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _controller = new OrdersController(_service.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task Create_Should_Return_CreatedAtAction()
        {
            _controller.HttpContext.Items[CorrelationHelper.HeaderName] = "corr-1";

            _service.Setup(x => x.CreateAsync(It.IsAny<CreateOrderRequestDto>(), "corr-1"))
                .ReturnsAsync(new OrderResponseDto { Id = Guid.NewGuid() });

            var result = await _controller.Create(new CreateOrderRequestDto());

            result.Result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_When_Missing()
        {
            _service.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((OrderResponseDto?)null);

            var result = await _controller.GetById(Guid.NewGuid());

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetById_Should_Set_ETag_Header_When_Available()
        {
            _service.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new OrderResponseDto
                {
                    Id = Guid.NewGuid(),
                    ETag = "abc123"
                });

            var result = await _controller.GetById(Guid.NewGuid());

            result.Result.Should().BeOfType<OkObjectResult>();
            _controller.Response.Headers.ETag.ToString().Should().Be("\"abc123\"");
        }

        [Fact]
        public async Task UpdateStatus_Should_Return_BadRequest_When_IdempotencyKey_Missing()
        {
            var result = await _controller.UpdateStatus(Guid.NewGuid(), new UpdateOrderStatusRequestDto());

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateStatus_Should_Return_NotFound_When_Order_Missing()
        {
            _controller.Request.Headers["Idempotency-Key"] = "key1";

            _service.Setup(x => x.UpdateStatusAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<UpdateOrderStatusRequestDto>(),
                    "key1"))
                .ReturnsAsync((OrderResponseDto?)null);

            var result = await _controller.UpdateStatus(Guid.NewGuid(), new UpdateOrderStatusRequestDto());

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateStatus_Should_Return_Ok_When_Updated()
        {
            _controller.Request.Headers["Idempotency-Key"] = "key1";

            _service.Setup(x => x.UpdateStatusAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<UpdateOrderStatusRequestDto>(),
                    "key1"))
                .ReturnsAsync(new OrderResponseDto
                {
                    Id = Guid.NewGuid(),
                    Status = "Paid"
                });

            var result = await _controller.UpdateStatus(Guid.NewGuid(), new UpdateOrderStatusRequestDto());

            result.Result.Should().BeOfType<OkObjectResult>();
        }
    }
}