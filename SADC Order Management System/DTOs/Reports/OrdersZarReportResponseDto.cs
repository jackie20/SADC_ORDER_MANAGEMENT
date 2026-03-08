namespace SADC_Order_Management_System.DTOs.Responses
{
    public class OrdersZarReportResponseDto
    {
        public decimal TotalAmountZar { get; set; }
        public string RoundingStrategy { get; set; } = "ToEven";
        public int CacheTtlSeconds { get; set; }
        public List<CurrencySummaryResponseDto> CurrencySummaries { get; set; } = new();
    }
}