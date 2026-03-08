namespace SADC_Order_Management_System.Helpers
{
    public static class CurrencyHelper
    {
        private static readonly Dictionary<string, HashSet<string>> AllowedPairs = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ZA"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ZAR" },
            ["BW"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "BWP" },
            ["ZW"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ZWL", "USD" },
            ["NA"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "NAD", "ZAR" },
            ["LS"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "LSL", "ZAR" },
            ["SZ"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "SZL", "ZAR" },
            ["ZM"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ZMW" },
            ["MZ"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "MZN" },
            ["AO"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "AOA" },
            ["MW"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "MWK" },
            ["TZ"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "TZS" },
            ["CD"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CDF" },
            ["MU"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "MUR" },
            ["SC"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "SCR" }
        };

        public static bool IsValidCountryCurrencyPair(string countryCode, string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(currencyCode))
            {
                return false;
            }

            return AllowedPairs.TryGetValue(countryCode.Trim().ToUpperInvariant(), out var currencies)
                   && currencies.Contains(currencyCode.Trim().ToUpperInvariant());
        }
    }
}