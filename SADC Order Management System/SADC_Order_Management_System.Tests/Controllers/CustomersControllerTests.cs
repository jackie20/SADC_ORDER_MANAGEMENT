using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SADC_Order_Management_System.Controllers;
using SADC_Order_Management_System.DTOs.Requests;
using SADC_Order_Management_System.DTOs.Responses;
using SADC_Order_Management_System.Services.Interfaces;
using Xunit;

namespace SADC_Order_Management_System.Tests.Controllers
{
    public class CustomersControllerTests
    {
        private readonly Mock<ICustomerService> _service = new();
        private readonly CustomersController _controller;

        public CustomersControllerTests()
        {
            _controller = new CustomersController(_service.Object);
        }

        [Fact]
        public async Task Create_Should_Return_CreatedAtAction()
        {
            var response = new CustomerResponseDto
            {
                Id = Guid.NewGuid(),
                Name = "John"
            };

            _service.Setup(x => x.CreateAsync(It.IsAny<CreateCustomerRequestDto>()))
                .ReturnsAsync(response);

            var result = await _controller.Create(new CreateCustomerRequestDto());

            result.Result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_When_Missing()
        {
            _service.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((CustomerResponseDto?)null);

            var result = await _controller.GetById(Guid.NewGuid());

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetById_Should_Return_Ok_When_Found()
        {
            _service.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new CustomerResponseDto { Id = Guid.NewGuid() });

            var result = await _controller.GetById(Guid.NewGuid());

            result.Result.Should().BeOfType<OkObjectResult>();
        }
    }
}