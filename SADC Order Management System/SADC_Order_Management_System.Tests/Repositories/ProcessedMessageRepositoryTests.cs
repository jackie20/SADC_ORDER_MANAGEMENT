using FluentAssertions;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Implementations;
using SADC_Order_Management_System.Tests.Helpers;
using Xunit;

namespace SADC_Order_Management_System.Tests.Repositories
{
    public class ProcessedMessageRepositoryTests
    {
        [Fact]
        public async Task ExistsAsync_Should_Return_True_When_Message_Exists()
        {
            using var db = InMemoryDbContextFactory.Create(Guid.NewGuid().ToString());

            db.ProcessedMessages.Add(new ProcessedMessage
            {
                MessageKey = "m1"
            });

            await db.SaveChangesAsync();

            var repo = new ProcessedMessageRepository(db);
            var result = await repo.ExistsAsync("m1");

            result.Should().BeTrue();
        }
    }
}