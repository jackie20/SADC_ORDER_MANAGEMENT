using FluentAssertions;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Implementations;
using SADC_Order_Management_System.Tests.Helpers;
using Xunit;

namespace SADC_Order_Management_System.Tests.Repositories
{
    public class OutboxRepositoryTests
    {
        [Fact]
        public async Task GetPendingAsync_Should_Return_Unprocessed_Messages()
        {
            using var db = InMemoryDbContextFactory.Create(Guid.NewGuid().ToString());

            db.OutboxMessages.AddRange(
                new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = "OrderCreated",
                    Payload = "{}",
                    ProcessedAtUtc = null
                },
                new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = "OrderCreated",
                    Payload = "{}",
                    ProcessedAtUtc = DateTime.UtcNow
                });

            await db.SaveChangesAsync();

            var repo = new OutboxRepository(db);
            var result = await repo.GetPendingAsync(10);

            result.Should().HaveCount(1);
        }
    }
}