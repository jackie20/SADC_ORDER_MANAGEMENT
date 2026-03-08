using SADC_Order_Management_System.DTOs.Requests;
using SADC_Order_Management_System.DTOs.Responses;

namespace SADC_Order_Management_System.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateAsync(CreateOrderRequestDto dto, string? correlationId);
        Task<OrderResponseDto?> GetByIdAsync(Guid id);
        Task<PagedResponseDto<OrderResponseDto>> GetPagedAsync(Guid? customerId, string? status, int page, int pageSize, string sort);
        Task<OrderResponseDto?> UpdateStatusAsync(Guid id, UpdateOrderStatusRequestDto dto, string idempotencyKey);
    }
}