using SADC_Order_Management_System.Models;

namespace SADC_Order_Management_System.Services.Interfaces
{
    public interface IIdempotencyService
    {
        Task<IdempotencyRecord?> GetAsync(string key);
        Task SaveAsync(string key, string path, string method, int statusCode, string responseBody);
    }
}