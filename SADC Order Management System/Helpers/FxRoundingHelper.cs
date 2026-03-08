namespace SADC_Order_Management_System.Helpers
{
    public static class FxRoundingHelper
    {
        public static decimal RoundMoney(decimal amount)
        {
            return decimal.Round(amount, 2, MidpointRounding.ToEven);
        }
    }
}