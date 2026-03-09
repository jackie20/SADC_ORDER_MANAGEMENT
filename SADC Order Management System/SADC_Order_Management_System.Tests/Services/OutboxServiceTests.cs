using Moq;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Interfaces;
using SADC_Order_Management_System.Services.Implementations;
using SADC_Order_Management_System.Tests.Helpers;
using Xunit;

namespace SADC_Order_Management_System.Tests.Services
{
    public class OutboxServiceTests
    {
        private readonly Mock<IOutboxRepository> _outboxRepository = new();
        private readonly OutboxService _service;

        public OutboxServiceTests()
        {
            _service = new OutboxService(_outboxRepository.Object);
        }

        [Fact]
        public async Task AddOrderCreatedMessageAsync_Should_Add_And_Save_Outbox_Record()
        {
            var order = TestDataBuilder.BuildOrder();

            await _service.AddOrderCreatedMessageAsync(order, "corr-123");

            _outboxRepository.Verify(x => x.AddAsync(It.Is<OutboxMessage>(m =>
                m.AggregateType == "Order" &&
                m.AggregateId == order.Id &&
                m.Type == "OrderCreated" &&
                m.CorrelationId == "corr-123")), Times.Once);

            _outboxRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}