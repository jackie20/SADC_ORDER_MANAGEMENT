using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SADC_Order_Management_System.Infrastructure.Data;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Interfaces;

namespace SADC_Order_Management_System.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;

        public OrderRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<Order?> GetByIdAsync(Guid id)
        {
            return _db.Orders
                .AsNoTracking()
                .Include(x => x.LineItems)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<(List<Order> Items, int Total)> GetPagedAsync(Guid? customerId, string? status, int page, int pageSize, string sort)
        {
            var query = _db.Orders
                .AsNoTracking()
                .Include(x => x.LineItems)
                .AsQueryable();

            if (customerId.HasValue)
            {
                query = query.Where(x => x.CustomerId == customerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsed))
            {
                query = query.Where(x => x.OrderStatus == parsed);
            }

            query = sort?.Trim().ToLowerInvariant() switch
            {
                "createdat_asc" => query.OrderBy(x => x.CreatedAt),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Order> CreateAsync(Order order)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return order;
        }

        public async Task UpdateAsync(Order order)
        {
            _db.Orders.Update(order);
            await _db.SaveChangesAsync();
        }
        public async Task<IDbContextTransactionProxy> BeginTransactionAsync()
        {

            var transaction = await _db.Database.BeginTransactionAsync();
            return new DbContextTransactionProxy(transaction);
        }

        private sealed class DbContextTransactionProxy : IDbContextTransactionProxy
        {
            private readonly IDbContextTransaction _transaction;

            public DbContextTransactionProxy(IDbContextTransaction transaction)
            {
                _transaction = transaction;
            }

            public async Task CommitAsync()
            {
                await _transaction.CommitAsync();
            }

            public async ValueTask DisposeAsync()
            {
                await _transaction.DisposeAsync();
            }
        }
    }
}