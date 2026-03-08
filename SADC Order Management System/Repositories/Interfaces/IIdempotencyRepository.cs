using SADC_Order_Management_System.Models;

namespace SADC_Order_Management_System.Repositories.Interfaces
{
    public interface IIdempotencyRepository
    {
        Task<IdempotencyRecord?> GetByKeyAsync(string key);
        Task SaveAsync(IdempotencyRecord record);
    }
}