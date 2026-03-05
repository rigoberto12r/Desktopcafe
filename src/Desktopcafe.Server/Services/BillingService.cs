using Desktopcafe.Core.Interfaces;
using Desktopcafe.Core.Models;
using Desktopcafe.Shared;

namespace Desktopcafe.Server.Services;

public class BillingService : IBillingService
{
    private readonly ICacheService _cacheService;

    public BillingService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public decimal CalculateSessionCharge(DateTime startTime, DateTime endTime, decimal ratePerHour, TimeSpan pausedDuration)
    {
        var totalTime = endTime - startTime;
        var billableTime = totalTime - pausedDuration;

        if (billableTime <= TimeSpan.Zero)
            return 0m;

        var hours = (decimal)billableTime.TotalHours;
        var charge = Math.Round(hours * ratePerHour, 2);
        return charge;
    }

    public RateConfig GetCurrentRate(bool isGaming = false)
    {
        var rates = _cacheService.GetRatesAsync().GetAwaiter().GetResult();
        var now = DateTime.Now;
        var isWeekend = now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        var hour = now.Hour;

        // Try to find a matching rate by day-of-week and time-of-day
        var matched = rates.FirstOrDefault(r =>
            r.IsWeekend == isWeekend &&
            r.IsNightRate == IsNightHour(hour) &&
            hour >= r.StartHour && hour < r.EndHour);

        if (matched is not null)
            return matched;

        // Fallback: match by weekend/night flags
        matched = rates.FirstOrDefault(r =>
            r.IsWeekend == isWeekend &&
            r.IsNightRate == IsNightHour(hour));

        if (matched is not null)
            return matched;

        // Fallback: default rate
        matched = rates.FirstOrDefault(r => r.IsDefault);

        return matched ?? new RateConfig
        {
            Name = "Default",
            PricePerHour = Constants.Defaults.PricePerHour,
            PricePerHalfHour = Constants.Defaults.PricePerHalfHour,
            PricePer15Min = Constants.Defaults.PricePer15Min,
            IsDefault = true
        };
    }

    public decimal CalculatePrintCost(int pages, bool isColor)
    {
        var pricePerPage = isColor
            ? Constants.Defaults.PrintPriceColor
            : Constants.Defaults.PrintPriceBW;
        return pages * pricePerPage;
    }

    private static bool IsNightHour(int hour) => hour >= 22 || hour < 6;
}
