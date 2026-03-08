using Microsoft.EntityFrameworkCore;
using SADC_Order_Management_System.Models;

namespace SADC_Order_Management_System.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderLineItem> OrderLineItems => Set<OrderLineItem>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
        public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderLineItem>()
                .Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .HasIndex(x => new { x.CustomerId, x.OrderStatus, x.CreatedAt })
                .HasDatabaseName("IX_Orders_CustomerId_Status_CreatedAt");

            modelBuilder.Entity<Order>()
                .HasOne(x => x.Customer)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderLineItem>()
                .HasOne(x => x.Order)
                .WithMany(x => x.LineItems)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OutboxMessage>()
                .HasIndex(x => new { x.ProcessedAtUtc, x.OccurredAtUtc });

            modelBuilder.Entity<ProcessedMessage>()
                .HasKey(x => x.MessageKey);

            modelBuilder.Entity<IdempotencyRecord>()
                .HasKey(x => x.IdempotencyKey);
        }
    }
}