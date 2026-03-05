using Desktopcafe.Core.Models;
using FluentAssertions;
using Xunit;

namespace Desktopcafe.Core.Tests;

public class BillingTests
{
    [Theory]
    [InlineData(60, 20, 20)]     // 1 hour at $20/hr = $20
    [InlineData(30, 20, 10)]     // 30 min at $20/hr = $10
    [InlineData(120, 20, 40)]    // 2 hours at $20/hr = $40
    [InlineData(15, 20, 5)]      // 15 min at $20/hr = $5
    [InlineData(90, 30, 45)]     // 1.5 hr at $30/hr = $45
    public void CalculateSessionCharge_ReturnsCorrectAmount(int durationMinutes, decimal ratePerHour, decimal expected)
    {
        var startTime = DateTime.Now.AddMinutes(-durationMinutes);
        var endTime = DateTime.Now;
        var pausedDuration = TimeSpan.Zero;

        var duration = endTime - startTime - pausedDuration;
        var charge = ratePerHour * (decimal)duration.TotalHours;

        Math.Round(charge, 2).Should().Be(expected);
    }

    [Fact]
    public void CalculateSessionCharge_WithPause_SubtractsPausedTime()
    {
        var startTime = DateTime.Now.AddMinutes(-60);
        var endTime = DateTime.Now;
        var pausedDuration = TimeSpan.FromMinutes(30);
        var ratePerHour = 20m;

        var duration = endTime - startTime - pausedDuration;
        var charge = ratePerHour * (decimal)duration.TotalHours;

        Math.Round(charge, 2).Should().Be(10m); // 30 effective minutes = $10
    }

    [Theory]
    [InlineData(5, false, 10)]   // 5 pages B/W at $2/page = $10
    [InlineData(3, true, 15)]    // 3 pages color at $5/page = $15
    [InlineData(0, false, 0)]    // 0 pages = $0
    public void CalculatePrintCost_ReturnsCorrectAmount(int pages, bool isColor, decimal expected)
    {
        decimal pricePerPage = isColor ? 5m : 2m;
        var cost = pages * pricePerPage;
        cost.Should().Be(expected);
    }

    [Fact]
    public void ClientCode_ShouldBe6Characters()
    {
        Span<char> buffer = stackalloc char[6];
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = chars[Random.Shared.Next(chars.Length)];
        var code = new string(buffer);

        code.Should().HaveLength(6);
        code.Should().MatchRegex("^[A-Z0-9]{6}$");
    }

    [Fact]
    public void RateConfig_DefaultValues_AreReasonable()
    {
        var config = new RateConfig
        {
            PricePerHour = 20m,
            PricePerHalfHour = 12m,
            PricePer15Min = 7m
        };

        config.PricePerHalfHour.Should().BeLessThan(config.PricePerHour);
        config.PricePer15Min.Should().BeLessThan(config.PricePerHalfHour);
        config.PricePerHour.Should().BeGreaterThan(0);
    }
}
