namespace SADC_Order_Management_System.Configurations
{
    public class FxOptions
    {
        public const string SectionName = "Fx";

        public string BaseCurrency { get; set; } = "ZAR";
        public int CacheTtlSeconds { get; set; } = 300;
        public string RoundingStrategy { get; set; } = "ToEven";
        public string ProviderMode { get; set; } = "DynamicMock";
    }
}