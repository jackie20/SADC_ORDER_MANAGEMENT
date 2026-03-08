namespace SADC_Order_Management_System.DTOs.Requests
{
    public class CreateOrderLineItemRequestDto
    {
        public string ProductSku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}