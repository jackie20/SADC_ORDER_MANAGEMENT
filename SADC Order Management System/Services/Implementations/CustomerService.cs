using SADC_Order_Management_System.DTOs.Requests;
using SADC_Order_Management_System.DTOs.Responses;
using SADC_Order_Management_System.Models;
using SADC_Order_Management_System.Repositories.Interfaces;
using SADC_Order_Management_System.Services.Interfaces;

namespace SADC_Order_Management_System.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var existing = await _customerRepository.GetByEmailAsync(email);
            if (existing != null)
            {
                throw new InvalidOperationException("Customer email already exists.");
            }

            var customer = new Customer
            {
                Name = dto.Name.Trim(),
                Email = email,
                CountryCode = dto.CountryCode.Trim().ToUpperInvariant(),
                CreatedAt = DateTime.UtcNow
            };

            await _customerRepository.CreateAsync(customer);

            return new CustomerResponseDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                CountryCode = customer.CountryCode,
                CreatedAt = customer.CreatedAt
            };
        }

        public async Task<CustomerResponseDto?> GetByIdAsync(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return null;
            }

            return new CustomerResponseDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                CountryCode = customer.CountryCode,
                CreatedAt = customer.CreatedAt
            };
        }

        public async Task<PagedResponseDto<CustomerResponseDto>> GetPagedAsync(string? search, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

            var (items, total) = await _customerRepository.GetPagedAsync(search, page, pageSize);

            return new PagedResponseDto<CustomerResponseDto>
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Data = items.Select(x => new CustomerResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    CountryCode = x.CountryCode,
                    CreatedAt = x.CreatedAt
                }).ToList()
            };



        }

    }

}