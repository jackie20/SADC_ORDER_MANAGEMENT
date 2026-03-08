using SADC_Order_Management_System.DTOs.Requests;
using SADC_Order_Management_System.DTOs.Responses;

namespace SADC_Order_Management_System.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto dto);
        Task<CustomerResponseDto?> GetByIdAsync(Guid id);
        Task<PagedResponseDto<CustomerResponseDto>> GetPagedAsync(string? search, int page, int pageSize);
    }
}