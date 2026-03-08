namespace SADC_Order_Management_System.Models
{
    public abstract class BaseEntity
    {
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
