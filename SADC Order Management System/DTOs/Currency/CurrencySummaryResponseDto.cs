namespace SADC_Order_Management_System.DTOs.Responses
{
    public class CurrencySummaryResponseDto
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal NativeAmount { get; set; }
        public decimal RateToZar { get; set; }
        public decimal ConvertedAmountZar { get; set; }
    }
}