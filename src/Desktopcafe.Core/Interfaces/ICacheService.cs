using Desktopcafe.Core.DTOs;
using Desktopcafe.Core.Models;

namespace Desktopcafe.Core.Interfaces;

public interface ICacheService
{
    Task<List<RateConfig>> GetRatesAsync();
    Task<List<Product>> GetActiveProductsAsync();
    Task<DashboardDto> GetDashboardAsync();
    void InvalidateRates();
    void InvalidateProducts();
    void InvalidateDashboard();
    void InvalidateAll();
    double HitRate { get; }
}
