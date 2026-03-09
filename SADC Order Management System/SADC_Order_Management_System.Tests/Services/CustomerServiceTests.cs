using FluentAssertions;
using Moq;
using SADC_Order_Management_System.DTOs.Requests;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Interfaces;
using SADC_Order_Management_System.Services.Implementations;
using Xunit;

namespace SADC_Order_Management_System.Tests.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepository = new();
        private readonly CustomerService _service;

        public CustomerServiceTests()
        {
            _service = new CustomerService(_customerRepository.Object);
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Customer_When_Email_Not_Exists()
        {
            var dto = new CreateCustomerRequestDto
            {
                Name = " John Doe ",
                Email = " JOHN@MAIL.COM ",
                CountryCode = " za "
            };

            _customerRepository.Setup(x => x.GetByEmailAsync("john@mail.com"))
                .ReturnsAsync((Customer?)null);

            _customerRepository.Setup(x => x.CreateAsync(It.IsAny<Customer>()))
                .ReturnsAsync((Customer c) => c);

            var result = await _service.CreateAsync(dto);

            result.Name.Should().Be("John Doe");
            result.Email.Should().Be("john@mail.com");
            result.CountryCode.Should().Be("ZA");

            _customerRepository.Verify(x => x.CreateAsync(It.IsAny<Customer>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Email_Already_Exists()
        {
            var dto = new CreateCustomerRequestDto
            {
                Name = "John Doe",
                Email = "john@mail.com",
                CountryCode = "ZA"
            };

            _customerRepository.Setup(x => x.GetByEmailAsync("john@mail.com"))
                .ReturnsAsync(new Customer());

            var action = async () => await _service.CreateAsync(dto);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Customer email already exists.");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            _customerRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Customer?)null);

            var result = await _service.GetByIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetPagedAsync_Should_Normalize_Invalid_Page_Values()
        {
            _customerRepository.Setup(x => x.GetPagedAsync(null, 1, 20))
                .ReturnsAsync((new List<Customer>(), 0));

            var result = await _service.GetPagedAsync(null, 0, 1000);

            result.Page.Should().Be(1);
            result.PageSize.Should().Be(20);
        }
    }
}