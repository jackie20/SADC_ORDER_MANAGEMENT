using SADC_Order_Management_System.Models;

namespace SADC_Order_Management_System.Repositories.Interfaces
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message);
        Task<List<OutboxMessage>> GetPendingAsync(int take);
        Task SaveChangesAsync();
    }
}