using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SADC_Order_Management_System.Configurations;
using SADC_Order_Management_System.DTOs.Responses;
using SADC_Order_Management_System.Helpers;
using SADC_Order_Management_System.Infrastructure.Data;
using SADC_Order_Management_System.Services.Interfaces;

namespace SADC_Order_Management_System.Services.Implementations
{
    public class FxService : IFxService
    {
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _db;
        private readonly FxOptions _options;
        private readonly ILogger<FxService> _logger;

        public FxService(
            IMemoryCache cache,
            AppDbContext db,
            IOptions<FxOptions> options,
            ILogger<FxService> logger)
        {
            _cache = cache;
            _db = db;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<decimal> GetRateToZarAsync(string currencyCode)
        {
            var code = currencyCode.Trim().ToUpperInvariant();
            var cacheKey = $"fx:{code}:zar";

            if (_cache.TryGetValue(cacheKey, out decimal cachedRate))
            {
                _logger.LogInformation("FX cache hit for {CurrencyCode}", code);
                return cachedRate;
            }

            _logger.LogInformation("FX cache miss for {CurrencyCode}", code);

            var dynamicRates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["ZAR"] = 1.0000m,
                ["BWP"] = 1.3700m,
                ["USD"] = 18.5000m,
                ["ZWL"] = 0.0510m,
                ["NAD"] = 1.0000m,
                ["LSL"] = 1.0000m,
                ["SZL"] = 1.0000m,
                ["ZMW"] = 0.7060m,
                ["MZN"] = 0.2890m,
                ["AOA"] = 0.0201m,
                ["MWK"] = 0.0107m,
                ["TZS"] = 0.0072m,
                ["CDF"] = 0.0065m,
                ["MUR"] = 0.4010m,
                ["SCR"] = 1.3520m,
            };

            if (!dynamicRates.TryGetValue(code, out var rate))
            {
                throw new InvalidOperationException($"FX rate not configured for currency '{code}'.");
            }

            _cache.Set(cacheKey, rate, TimeSpan.FromSeconds(_options.CacheTtlSeconds));
            return await Task.FromResult(rate);

        }


        public async Task<OrdersZarReportResponseDto> GetOrdersZarReportAsync()
        {
            var currencyTotals = await _db.Orders
                .AsNoTracking()
                .GroupBy(x => x.CurrencyCode)
                .Select(g => new
                {
                    CurrencyCode = g.Key,
                    OrderCount = g.Count(),
                    NativeAmount = g.Sum(x => x.TotalAmount)
                })
                .ToListAsync();

            var summaries = new List<CurrencySummaryResponseDto>();

            foreach (var item in currencyTotals)
            {
                var rate = await GetRateToZarAsync(item.CurrencyCode);
                var converted = FxRoundingHelper.RoundMoney(item.NativeAmount * rate);

                summaries.Add(new CurrencySummaryResponseDto
                {
                    CurrencyCode = item.CurrencyCode,
                    OrderCount = item.OrderCount,
                    NativeAmount = FxRoundingHelper.RoundMoney(item.NativeAmount),
                    RateToZar = rate,
                    ConvertedAmountZar = converted
                });
            }

            return new OrdersZarReportResponseDto
            {
                CacheTtlSeconds = _options.CacheTtlSeconds,
                RoundingStrategy = _options.RoundingStrategy,
                TotalAmountZar = FxRoundingHelper.RoundMoney(summaries.Sum(x => x.ConvertedAmountZar)),
                CurrencySummaries = summaries.OrderBy(x => x.CurrencyCode).ToList()
            };
        }



    }

}