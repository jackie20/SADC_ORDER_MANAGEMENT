using Microsoft.EntityFrameworkCore;
using SADC_Order_Management_System.Infrastructure.Data;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Interfaces;

namespace SADC_Order_Management_System.Repositories.Implementations
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _db;

        public CustomerRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<Customer?> GetByIdAsync(Guid id)
        {
            return _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Customer?> GetByEmailAsync(string email)
        {
            return _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<(List<Customer> Items, int Total)> GetPagedAsync(string? search, int page, int pageSize)
        {
            var query = _db.Customers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x => x.Name.Contains(term) || x.Email.Contains(term));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();
            return customer;
        }
    }
}