using System.Text.Json;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Interfaces;
using SADC_Order_Management_System.Services.Interfaces;

namespace SADC_Order_Management_System.Services.Implementations
{
    public class OutboxService : IOutboxService
    {
        private readonly IOutboxRepository _outboxRepository;

        public OutboxService(IOutboxRepository outboxRepository)
        {
            _outboxRepository = outboxRepository;
        }

        public async Task AddOrderCreatedMessageAsync(Order order, string? correlationId)
        {
            var payload = JsonSerializer.Serialize(new
            {
                MessageId = Guid.NewGuid(),
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                CurrencyCode = order.CurrencyCode,
                TotalAmount = order.TotalAmount,
                OccurredAtUtc = DateTime.UtcNow,
                Version = 1
            });

            var outbox = new OutboxMessage
            {
                AggregateType = "Order",
                AggregateId = order.Id,
                Type = "OrderCreated",
                Payload = payload,
                OccurredAtUtc = DateTime.UtcNow,
                Version = 1,
                CorrelationId = correlationId
            };

            await _outboxRepository.AddAsync(outbox);
            await _outboxRepository.SaveChangesAsync();
        }
    }
}