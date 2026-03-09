using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SADC_Order_Management_System.Configurations;
using SADC_Order_Management_System.Infrastructure.Data;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Services.Implementations;
using SADC_Order_Management_System.Tests.Helpers;
using Xunit;

namespace SADC_Order_Management_System.Tests.Services
{
    public class FxServiceTests
    {
        [Fact]
        public async Task GetRateToZarAsync_Should_Return_Configured_Rate()
        {
            using var db = InMemoryDbContextFactory.Create(Guid.NewGuid().ToString());
            var cache = new MemoryCache(new MemoryCacheOptions());
            var options = Options.Create(new FxOptions { CacheTtlSeconds = 300, RoundingStrategy = "ToEven" });
            var logger = LoggerFactory.Create(builder => { }).CreateLogger<FxService>();

            var service = new FxService(cache, db, options, logger);

            var result = await service.GetRateToZarAsync("USD");

            result.Should().Be(18.5000m);
        }

        [Fact]
        public async Task GetRateToZarAsync_Should_Use_Cache_On_Second_Call()
        {
            using var db = InMemoryDbContextFactory.Create(Guid.NewGuid().ToString());
            var cache = new MemoryCache(new MemoryCacheOptions());
            var options = Options.Create(new FxOptions { CacheTtlSeconds = 300, RoundingStrategy = "ToEven" });
            var logger = LoggerFactory.Create(builder => { }).CreateLogger<FxService>();

            var service = new FxService(cache, db, options, logger);

            var first = await service.GetRateToZarAsync("BWP");
            var second = await service.GetRateToZarAsync("BWP");

            first.Should().Be(second);
        }

        [Fact]
        public async Task GetOrdersZarReportAsync_Should_Group_And_Convert_Currencies()
        {
            using var db = InMemoryDbContextFactory.Create(Guid.NewGuid().ToString());

            db.Orders.AddRange(
                new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    CurrencyCode = "ZAR",
                    OrderStatus = OrderStatus.Pending,
                    TotalAmount = 100m,
                    CreatedAt = DateTime.UtcNow
                },
                new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid(),
                    CurrencyCode = "USD",
                    OrderStatus = OrderStatus.Pending,
                    TotalAmount = 10m,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await db.SaveChangesAsync();

            var cache = new MemoryCache(new MemoryCacheOptions());
            var options = Options.Create(new FxOptions { CacheTtlSeconds = 300, RoundingStrategy = "ToEven" });
            var logger = LoggerFactory.Create(builder => { }).CreateLogger<FxService>();

            var service = new FxService(cache, db, options, logger);

            var result = await service.GetOrdersZarReportAsync();

            result.CurrencySummaries.Should().HaveCount(2);
            result.TotalAmountZar.Should().Be(285.00m);
        }
    }
}