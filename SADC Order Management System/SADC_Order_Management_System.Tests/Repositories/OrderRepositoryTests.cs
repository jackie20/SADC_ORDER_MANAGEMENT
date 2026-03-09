using FluentAssertions;
using SADC_Order_Management_System.Repositories.Implementations;
using SADC_Order_Management_System.Tests.Helpers;
using Xunit;

namespace SADC_Order_Management_System.Tests.Repositories
{
    public class OrderRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_Should_Return_Order_With_LineItems()
        {
            using var db = InMemoryDbContextFactory.Create(Guid.NewGuid().ToString());

            var order = TestDataBuilder.BuildOrder();
            db.Orders.Add(order);
            db.OrderLineItems.AddRange(order.LineItems);
            await db.SaveChangesAsync();

            var repo = new OrderRepository(db);
            var result = await repo.GetByIdAsync(order.Id);

            result.Should().NotBeNull();
            result!.LineItems.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateAsync_Should_Save_Order()
        {
            using var db = InMemoryDbContextFactory.Create(Guid.NewGuid().ToString());
            var repo = new OrderRepository(db);
            var order = TestDataBuilder.BuildOrder();

            var result = await repo.CreateAsync(order);

            result.Id.Should().Be(order.Id);
            db.Orders.Count().Should().Be(1);
        }
    }
}