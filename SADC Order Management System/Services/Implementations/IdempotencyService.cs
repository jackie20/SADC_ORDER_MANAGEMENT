using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Interfaces;
using SADC_Order_Management_System.Services.Interfaces;

namespace SADC_Order_Management_System.Services.Implementations
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly IIdempotencyRepository _repository;

        public IdempotencyService(IIdempotencyRepository repository)
        {
            _repository = repository;
        }

        public Task<IdempotencyRecord?> GetAsync(string key)
        {
            return _repository.GetByKeyAsync(key);
        }

        public Task SaveAsync(string key, string path, string method, int statusCode, string responseBody)
        {
            return _repository.SaveAsync(new IdempotencyRecord
            {
                IdempotencyKey = key,
                RequestPath = path,
                HttpMethod = method,
                StatusCode = statusCode,
                ResponseBody = responseBody,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
    }
}