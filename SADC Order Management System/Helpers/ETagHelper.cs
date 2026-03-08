namespace SADC_Order_Management_System.Helpers
{
    public static class ETagHelper
    {
        public static string FromRowVersion(byte[] rowVersion)
        {
            return Convert.ToBase64String(rowVersion);
        }
    }
}