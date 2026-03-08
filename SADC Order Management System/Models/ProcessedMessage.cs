using System.ComponentModel.DataAnnotations;

namespace SADC_Order_Management_System.Models
{
    public class ProcessedMessage
    {
        [Key]
        [MaxLength(200)]
        public string MessageKey { get; set; } = string.Empty;

        public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
    }
}