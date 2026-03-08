namespace SADC_Order_Management_System.Infrastructure.Messaging
{
    public class OrderCreatedEvent
    {
        public Guid MessageId { get; set; }
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public int Version { get; set; }
    }
}