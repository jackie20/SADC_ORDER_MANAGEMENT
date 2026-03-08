using Microsoft.EntityFrameworkCore;
using SADC_Order_Management_System.Infrastructure.Data;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Interfaces;

namespace SADC_Order_Management_System.Repositories.Implementations
{
    public class ProcessedMessageRepository : IProcessedMessageRepository
    {
        private readonly AppDbContext _db;

        public ProcessedMessageRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<bool> ExistsAsync(string messageKey)
        {
            return _db.ProcessedMessages.AnyAsync(x => x.MessageKey == messageKey);
        }

        public async Task AddAsync(ProcessedMessage message)
        {
            _db.ProcessedMessages.Add(message);
            await Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}