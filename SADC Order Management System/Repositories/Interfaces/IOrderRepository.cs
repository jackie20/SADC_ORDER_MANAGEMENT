using SADC_Order_Management_System.Models;

namespace SADC_Order_Management_System.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id);
        Task<(List<Order> Items, int Total)> GetPagedAsync(Guid? customerId, string? status, int page, int pageSize, string sort);
        Task<Order> CreateAsync(Order order);
        Task UpdateAsync(Order order);
        Task<IDbContextTransactionProxy> BeginTransactionAsync();
    }

    public interface IDbContextTransactionProxy : IAsyncDisposable
    {
        Task CommitAsync();
    }
}