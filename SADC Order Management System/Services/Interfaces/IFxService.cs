using SADC_Order_Management_System.DTOs.Responses;

namespace SADC_Order_Management_System.Services.Interfaces
{
    public interface IFxService
    {
        Task<decimal> GetRateToZarAsync(string currencyCode);
        Task<OrdersZarReportResponseDto> GetOrdersZarReportAsync();
    }
}