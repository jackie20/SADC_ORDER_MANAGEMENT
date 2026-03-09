using FluentAssertions;
using Moq;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Interfaces;
using SADC_Order_Management_System.Services.Implementations;
using Xunit;

namespace SADC_Order_Management_System.Tests.Services
{
    public class IdempotencyServiceTests
    {
        private readonly Mock<IIdempotencyRepository> _repository = new();
        private readonly IdempotencyService _service;

        public IdempotencyServiceTests()
        {
            _service = new IdempotencyService(_repository.Object);
        }

        [Fact]
        public async Task GetAsync_Should_Return_Record()
        {
            var record = new IdempotencyRecord { IdempotencyKey = "abc" };
            _repository.Setup(x => x.GetByKeyAsync("abc")).ReturnsAsync(record);

            var result = await _service.GetAsync("abc");

            result.Should().NotBeNull();
            result!.IdempotencyKey.Should().Be("abc");
        }

        [Fact]
        public async Task SaveAsync_Should_Save_Record()
        {
            await _service.SaveAsync("key1", "/api/orders/1/status", "PUT", 200, "{}");

            _repository.Verify(x => x.SaveAsync(It.Is<IdempotencyRecord>(r =>
                r.IdempotencyKey == "key1" &&
                r.RequestPath == "/api/orders/1/status" &&
                r.HttpMethod == "PUT" &&
                r.StatusCode == 200)), Times.Once);
        }
    }
}