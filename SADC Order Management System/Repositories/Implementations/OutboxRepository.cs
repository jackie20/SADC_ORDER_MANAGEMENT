using Microsoft.EntityFrameworkCore;
using SADC_Order_Management_System.Infrastructure.Data;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Interfaces;

namespace SADC_Order_Management_System.Repositories.Implementations
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly AppDbContext _db;

        public OutboxRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(OutboxMessage message)
        {
            _db.OutboxMessages.Add(message);
            await Task.CompletedTask;
        }

        public Task<List<OutboxMessage>> GetPendingAsync(int take)
        {
            return _db.OutboxMessages
                .Where(x => x.ProcessedAtUtc == null)
                .OrderBy(x => x.OccurredAtUtc)
                .Take(take)
                .ToListAsync();
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}