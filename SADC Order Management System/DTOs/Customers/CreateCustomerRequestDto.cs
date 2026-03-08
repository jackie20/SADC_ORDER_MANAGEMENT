namespace SADC_Order_Management_System.DTOs.Requests
{
    public class CreateCustomerRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
    }
}