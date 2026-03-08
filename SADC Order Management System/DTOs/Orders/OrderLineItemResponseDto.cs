namespace SADC_Order_Management_System.DTOs.Responses
{
    public class OrderLineItemResponseDto
    {
        public Guid Id { get; set; }
        public string ProductSku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}