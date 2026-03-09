using SADC_Order_Management_System.Models;

namespace SADC_Order_Management_System.Tests.Helpers
{
    public static class TestDataBuilder
    {
        public static Customer BuildCustomer(
            string countryCode = "ZA",
            string email = "user@test.com")
        {
            return new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Test Customer",
                Email = email,
                CountryCode = countryCode,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Status = "active"
            };
        }

        public static Order BuildOrder(
            Guid? customerId = null,
            OrderStatus status = OrderStatus.Pending,
            string currencyCode = "ZAR")
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId ?? Guid.NewGuid(),
                CurrencyCode = currencyCode,
                OrderStatus = status,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = 250.00m,
                IsActive = true,
                Status = "active",
                RowVersion = new byte[] { 1, 2, 3 }
            };

            order.LineItems = new List<OrderLineItem>
            {
                new OrderLineItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductSku = "SKU001",
                    Quantity = 2,
                    UnitPrice = 100.00m,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Status = "active"
                },
                new OrderLineItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductSku = "SKU002",
                    Quantity = 1,
                    UnitPrice = 50.00m,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Status = "active"
                }
            };

            return order;
        }
    }
}