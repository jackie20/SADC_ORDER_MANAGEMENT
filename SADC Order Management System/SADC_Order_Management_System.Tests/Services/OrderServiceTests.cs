using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SADC_Order_Management_System.DTOs.Requests;
using SADC_Order_Management_System.DTOs.Responses;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Interfaces;
using SADC_Order_Management_System.Services.Implementations;
using SADC_Order_Management_System.Services.Interfaces;
using SADC_Order_Management_System.Tests.Helpers;
using System.Text.Json;
using Xunit;

namespace SADC_Order_Management_System.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepository = new();
        private readonly Mock<ICustomerRepository> _customerRepository = new();
        private readonly Mock<IOutboxRepository> _outboxRepository = new();
        private readonly Mock<IIdempotencyService> _idempotencyService = new();
        private readonly Mock<ILogger<OrderService>> _logger = new();
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _service = new OrderService(
                _orderRepository.Object,
                _customerRepository.Object,
                _outboxRepository.Object,
                _idempotencyService.Object,
                _logger.Object);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Customer_Not_Found()
        {
            var dto = new CreateOrderRequestDto
            {
                CustomerId = Guid.NewGuid(),
                CurrencyCode = "ZAR",
                LineItems = new List<CreateOrderLineItemRequestDto>
                {
                    new() { ProductSku = "SKU1", Quantity = 1, UnitPrice = 100m }
                }
            };

            _customerRepository.Setup(x => x.GetByIdAsync(dto.CustomerId))
                .ReturnsAsync((Customer?)null);

            var action = async () => await _service.CreateAsync(dto, "corr1");

            await action.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Currency_Pair_Is_Invalid()
        {
            var customer = TestDataBuilder.BuildCustomer(countryCode: "ZA");

            var dto = new CreateOrderRequestDto
            {
                CustomerId = customer.Id,
                CurrencyCode = "USD",
                LineItems = new List<CreateOrderLineItemRequestDto>
                {
                    new() { ProductSku = "SKU1", Quantity = 1, UnitPrice = 100m }
                }
            };

            _customerRepository.Setup(x => x.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            var action = async () => await _service.CreateAsync(dto, "corr1");

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Invalid country/currency pairing*");
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_No_Line_Items()
        {
            var customer = TestDataBuilder.BuildCustomer();

            var dto = new CreateOrderRequestDto
            {
                CustomerId = customer.Id,
                CurrencyCode = "ZAR",
                LineItems = new List<CreateOrderLineItemRequestDto>()
            };

            _customerRepository.Setup(x => x.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            var action = async () => await _service.CreateAsync(dto, "corr1");

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Order must contain at least one line item.");
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Quantity_Is_Invalid()
        {
            var customer = TestDataBuilder.BuildCustomer();

            var dto = new CreateOrderRequestDto
            {
                CustomerId = customer.Id,
                CurrencyCode = "ZAR",
                LineItems = new List<CreateOrderLineItemRequestDto>
                {
                    new() { ProductSku = "SKU1", Quantity = 0, UnitPrice = 100m }
                }
            };

            _customerRepository.Setup(x => x.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            var action = async () => await _service.CreateAsync(dto, "corr1");

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Quantity must be greater than zero.");
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Order_And_Outbox_When_Request_Is_Valid()
        {
            var customer = TestDataBuilder.BuildCustomer(countryCode: "ZA");
            var tx = new Mock<IDbContextTransactionProxy>();

            var dto = new CreateOrderRequestDto
            {
                CustomerId = customer.Id,
                CurrencyCode = "ZAR",
                LineItems = new List<CreateOrderLineItemRequestDto>
                {
                    new() { ProductSku = "SKU1", Quantity = 2, UnitPrice = 100m },
                    new() { ProductSku = "SKU2", Quantity = 1, UnitPrice = 50m }
                }
            };

            _customerRepository.Setup(x => x.GetByIdAsync(customer.Id))
                .ReturnsAsync(customer);

            _orderRepository.Setup(x => x.BeginTransactionAsync())
                .ReturnsAsync(tx.Object);

            _orderRepository.Setup(x => x.CreateAsync(It.IsAny<Order>()))
                .ReturnsAsync((Order o) => o);

            var result = await _service.CreateAsync(dto, "corr-123");

            result.TotalAmount.Should().Be(250m);
            result.CurrencyCode.Should().Be("ZAR");
            result.Status.Should().Be(OrderStatus.Pending.ToString());

            _orderRepository.Verify(x => x.CreateAsync(It.IsAny<Order>()), Times.Once);
            _outboxRepository.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>()), Times.Once);
            _outboxRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
            tx.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Order_Does_Not_Exist()
        {
            _orderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Order?)null);

            var result = await _service.GetByIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Mapped_Order_When_Found()
        {
            var order = TestDataBuilder.BuildOrder();

            _orderRepository.Setup(x => x.GetByIdAsync(order.Id))
                .ReturnsAsync(order);

            var result = await _service.GetByIdAsync(order.Id);

            result.Should().NotBeNull();
            result!.LineItems.Should().HaveCount(2);
            result.TotalAmount.Should().Be(order.TotalAmount);
        }

        [Fact]
        public async Task GetPagedAsync_Should_Normalize_Page_Values()
        {
            _orderRepository.Setup(x => x.GetPagedAsync(null, null, 1, 20, "createdAt_desc"))
                .ReturnsAsync((new List<Order>(), 0));

            var result = await _service.GetPagedAsync(null, null, 0, 1000, "createdAt_desc");

            result.Page.Should().Be(1);
            result.PageSize.Should().Be(20);
        }

        [Fact]
        public async Task UpdateStatusAsync_Should_Throw_When_IdempotencyKey_Is_Missing()
        {
            var dto = new UpdateOrderStatusRequestDto { Status = "Paid" };

            var action = async () => await _service.UpdateStatusAsync(Guid.NewGuid(), dto, "");

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Idempotency-Key header is required.");
        }

        [Fact]
        public async Task UpdateStatusAsync_Should_Return_Cached_Response_When_Idempotency_Exists()
        {
            var order = TestDataBuilder.BuildOrder(status: OrderStatus.Paid);

            var cachedResponse = new OrderResponseDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = "Paid",
                CurrencyCode = order.CurrencyCode,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt
            };

            _idempotencyService.Setup(x => x.GetAsync("key1"))
                .ReturnsAsync(new IdempotencyRecord
                {
                    IdempotencyKey = "key1",
                    ResponseBody = JsonSerializer.Serialize(cachedResponse)
                });

            var result = await _service.UpdateStatusAsync(
                order.Id,
                new UpdateOrderStatusRequestDto { Status = "Paid" },
                "key1");

            result.Should().NotBeNull();
            result!.Status.Should().Be("Paid");

            _orderRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatusAsync_Should_Return_Null_When_Order_Not_Found()
        {
            _idempotencyService.Setup(x => x.GetAsync("key1"))
                .ReturnsAsync((IdempotencyRecord?)null);

            _orderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Order?)null);

            var result = await _service.UpdateStatusAsync(
                Guid.NewGuid(),
                new UpdateOrderStatusRequestDto { Status = "Paid" },
                "key1");

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateStatusAsync_Should_Throw_When_Status_Value_Is_Invalid()
        {
            var order = TestDataBuilder.BuildOrder(status: OrderStatus.Pending);

            _idempotencyService.Setup(x => x.GetAsync("key1"))
                .ReturnsAsync((IdempotencyRecord?)null);

            _orderRepository.Setup(x => x.GetByIdAsync(order.Id))
                .ReturnsAsync(order);

            var action = async () => await _service.UpdateStatusAsync(
                order.Id,
                new UpdateOrderStatusRequestDto { Status = "BadStatus" },
                "key1");

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid status value.");
        }

        [Fact]
        public async Task UpdateStatusAsync_Should_Throw_When_Transition_Is_Invalid()
        {
            var order = TestDataBuilder.BuildOrder(status: OrderStatus.Fulfilled);

            _idempotencyService.Setup(x => x.GetAsync("key1"))
                .ReturnsAsync((IdempotencyRecord?)null);

            _orderRepository.Setup(x => x.GetByIdAsync(order.Id))
                .ReturnsAsync(order);

            var action = async () => await _service.UpdateStatusAsync(
                order.Id,
                new UpdateOrderStatusRequestDto { Status = "Cancelled" },
                "key1");

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Invalid status transition*");
        }

        [Fact]
        public async Task UpdateStatusAsync_Should_Update_And_Save_Idempotency_When_Valid()
        {
            var order = TestDataBuilder.BuildOrder(status: OrderStatus.Pending);

            _idempotencyService.Setup(x => x.GetAsync("key1"))
                .ReturnsAsync((IdempotencyRecord?)null);

            _orderRepository.Setup(x => x.GetByIdAsync(order.Id))
                .ReturnsAsync(order);

            var result = await _service.UpdateStatusAsync(
                order.Id,
                new UpdateOrderStatusRequestDto { Status = "Paid" },
                "key1");

            result.Should().NotBeNull();
            result!.Status.Should().Be("Paid");

            _orderRepository.Verify(x => x.UpdateAsync(It.Is<Order>(o => o.OrderStatus == OrderStatus.Paid)), Times.Once);

            _idempotencyService.Verify(x => x.SaveAsync(
                "key1",
                $"/api/orders/{order.Id}/status",
                "PUT",
                200,
                It.IsAny<string>()), Times.Once);
        }
    }
}