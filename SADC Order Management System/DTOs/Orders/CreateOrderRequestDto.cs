namespace SADC_Order_Management_System.DTOs.Requests
{
    public class CreateOrderRequestDto
    {
        public Guid CustomerId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public List<CreateOrderLineItemRequestDto> LineItems { get; set; } = new();
    }
}