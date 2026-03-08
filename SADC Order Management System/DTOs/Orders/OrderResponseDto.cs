namespace SADC_Order_Management_System.DTOs.Responses
{
    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string ETag { get; set; } = string.Empty;
        public List<OrderLineItemResponseDto> LineItems { get; set; } = new();
    }
}