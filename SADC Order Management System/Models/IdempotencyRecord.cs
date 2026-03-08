using System.ComponentModel.DataAnnotations;

namespace SADC_Order_Management_System.Models
{
    public class IdempotencyRecord
    {
        [Key]
        [MaxLength(200)]
        public string IdempotencyKey { get; set; } = string.Empty;

        [MaxLength(200)]
        public string RequestPath { get; set; } = string.Empty;

        [MaxLength(20)]
        public string HttpMethod { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public string ResponseBody { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
