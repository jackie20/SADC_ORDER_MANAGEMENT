using Microsoft.EntityFrameworkCore;
using SADC_Order_Management_System.Infrastructure.Data;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Interfaces;

namespace SADC_Order_Management_System.Repositories.Implementations
{
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly AppDbContext _db;

        public IdempotencyRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<IdempotencyRecord?> GetByKeyAsync(string key)
        {
            return _db.IdempotencyRecords.FirstOrDefaultAsync(x => x.IdempotencyKey == key);
        }

        public async Task SaveAsync(IdempotencyRecord record)
        {
            _db.IdempotencyRecords.Add(record);
            await _db.SaveChangesAsync();
        }
    }
}