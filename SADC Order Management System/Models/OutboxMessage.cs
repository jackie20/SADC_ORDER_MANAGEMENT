using System.ComponentModel.DataAnnotations;

namespace SADC_Order_Management_System.Models
{
    public class OutboxMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string AggregateType { get; set; } = "Order";

        public Guid AggregateId { get; set; }

        [Required]
        public string Type { get; set; } = "OrderCreated";

        [Required]
        public string Payload { get; set; } = "{}";

        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAtUtc { get; set; }
        public int Version { get; set; } = 1;
        public string? CorrelationId { get; set; }
        public int RetryCount { get; set; }
        public DateTime? LastAttemptAtUtc { get; set; }
    }
}
