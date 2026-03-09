using FluentAssertions;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Implementations;
using SADC_Order_Management_System.Tests.Helpers;
using Xunit;

namespace SADC_Order_Management_System.Tests.Repositories
{
    public class IdempotencyRepositoryTests
    {
        [Fact]
        public async Task SaveAsync_Should_Persist_Record()
        {
            using var db = InMemoryDbContextFactory.Create(Guid.NewGuid().ToString());
            var repo = new IdempotencyRepository(db);

            await repo.SaveAsync(new IdempotencyRecord
            {
                IdempotencyKey = "key1",
                RequestPath = "/api/orders/1/status",
                HttpMethod = "PUT",
                StatusCode = 200,
                ResponseBody = "{}"
            });

            db.IdempotencyRecords.Count().Should().Be(1);
        }
    }
}