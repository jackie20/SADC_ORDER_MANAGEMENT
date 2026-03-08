namespace SADC_Order_Management_System.DTOs.Responses
{
    public class CustomerResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}