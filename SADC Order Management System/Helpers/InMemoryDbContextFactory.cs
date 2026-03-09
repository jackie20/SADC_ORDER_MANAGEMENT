using Microsoft.EntityFrameworkCore;
using SADC_Order_Management_System.Infrastructure.Data;

namespace SADC_Order_Management_System.Tests.Helpers
{
    public static class InMemoryDbContextFactory
    {
        public static AppDbContext Create(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new AppDbContext(options);
        }
    }
}