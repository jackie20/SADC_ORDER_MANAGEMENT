using SADC_Order_Management_System.Models;

namespace SADC_Order_Management_System.Repositories.Interfaces
{
    public interface IProcessedMessageRepository
    {
        Task<bool> ExistsAsync(string messageKey);
        Task AddAsync(ProcessedMessage message);
        Task SaveChangesAsync();
    }
}