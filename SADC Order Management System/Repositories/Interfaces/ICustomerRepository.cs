using SADC_Order_Management_System.Models;

namespace SADC_Order_Management_System.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid id);
        Task<Customer?> GetByEmailAsync(string email);
        Task<(List<Customer> Items, int Total)> GetPagedAsync(string? search, int page, int pageSize);
        Task<Customer> CreateAsync(Customer customer);
    }
}