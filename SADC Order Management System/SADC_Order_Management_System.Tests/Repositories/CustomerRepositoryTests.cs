using FluentAssertions;
using SADC_Order_Management_System.Repositories.Implementations;
using SADC_Order_Management_System.Tests.Helpers;
using Xunit;

namespace SADC_Order_Management_System.Tests.Repositories
{
    public class CustomerRepositoryTests
    {
        [Fact]
        public async Task CreateAsync_Should_Save_Customer()
        {
            using var db = InMemoryDbContextFactory.Create(Guid.NewGuid().ToString());
            var repo = new CustomerRepository(db);
            var customer = TestDataBuilder.BuildCustomer();

            var result = await repo.CreateAsync(customer);

            result.Id.Should().Be(customer.Id);
            db.Customers.Count().Should().Be(1);
        }

        [Fact]
        public async Task GetByEmailAsync_Should_Return_Customer_When_Exists()
        {
            using var db = InMemoryDbContextFactory.Create(Guid.NewGuid().ToString());
            var customer = TestDataBuilder.BuildCustomer(email: "repo@test.com");
            db.Customers.Add(customer);
            await db.SaveChangesAsync();

            var repo = new CustomerRepository(db);
            var result = await repo.GetByEmailAsync("repo@test.com");

            result.Should().NotBeNull();
        }
    }
}