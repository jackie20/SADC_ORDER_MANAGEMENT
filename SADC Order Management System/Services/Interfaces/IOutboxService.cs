using SADC_Order_Management_System.Models;

namespace SADC_Order_Management_System.Services.Interfaces
{
    public interface IOutboxService
    {
        Task AddOrderCreatedMessageAsync(Order order, string? correlationId);
    }
}