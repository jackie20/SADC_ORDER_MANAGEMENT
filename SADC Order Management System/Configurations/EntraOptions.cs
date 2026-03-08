namespace SADC_Order_Management_System.Configurations
{
    public class EntraOptions
    {
        public const string SectionName = "MicrosoftEntra";

        public string Instance { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}