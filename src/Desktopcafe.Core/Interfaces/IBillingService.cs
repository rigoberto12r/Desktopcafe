using Desktopcafe.Core.Enums;
using Desktopcafe.Core.Models;

namespace Desktopcafe.Core.Interfaces;

public interface IBillingService
{
    decimal CalculateSessionCharge(DateTime startTime, DateTime endTime, decimal ratePerHour, TimeSpan pausedDuration);
    RateConfig GetCurrentRate(bool isGaming = false);
    decimal CalculatePrintCost(int pages, bool isColor);
}
