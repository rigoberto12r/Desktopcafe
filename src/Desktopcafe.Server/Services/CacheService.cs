using System.Collections.Concurrent;
using Desktopcafe.Core.DTOs;
using Desktopcafe.Core.Interfaces;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Desktopcafe.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Desktopcafe.Server.Services;

public class CacheService : ICacheService
{
    private const string RatesKey = "cache:rates";
    private const string ProductsKey = "cache:active_products";
    private const string DashboardKey = "cache:dashboard";

    private static readonly TimeSpan RatesTtl = TimeSpan.FromHours(1);
    private static readonly TimeSpan ProductsTtl = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan DashboardTtl = TimeSpan.FromSeconds(30);

    private readonly IMemoryCache _memoryCache;
    private readonly AppDbContext _db;
    private readonly ReportRepository _reportRepository;

    private readonly ConcurrentDictionary<string, object?> _staticCache = new();

    private long _hits;
    private long _misses;

    public double HitRate
    {
        get
        {
            var total = Interlocked.Read(ref _hits) + Interlocked.Read(ref _misses);
            return total == 0 ? 0 : (double)Interlocked.Read(ref _hits) / total * 100;
        }
    }

    public CacheService(IMemoryCache memoryCache, AppDbContext db, ReportRepository reportRepository)
    {
        _memoryCache = memoryCache;
        _db = db;
        _reportRepository = reportRepository;
    }

    public async Task<List<RateConfig>> GetRatesAsync()
    {
        if (_memoryCache.TryGetValue(RatesKey, out List<RateConfig>? cached) && cached is not null)
        {
            Interlocked.Increment(ref _hits);
            return cached;
        }

        Interlocked.Increment(ref _misses);

        try
        {
            var rates = await _db.RateConfigs.AsNoTracking().ToListAsync();
            _memoryCache.Set(RatesKey, rates, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = RatesTtl,
                Size = 1
            });
            _staticCache[RatesKey] = rates;
            return rates;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load rates from database");
            throw;
        }
    }

    public async Task<List<Product>> GetActiveProductsAsync()
    {
        if (_memoryCache.TryGetValue(ProductsKey, out List<Product>? cached) && cached is not null)
        {
            Interlocked.Increment(ref _hits);
            return cached;
        }

        Interlocked.Increment(ref _misses);

        try
        {
            var products = await _db.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .ToListAsync();
            _memoryCache.Set(ProductsKey, products, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ProductsTtl,
                Size = 1
            });
            _staticCache[ProductsKey] = products;
            return products;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load active products from database");
            throw;
        }
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        if (_memoryCache.TryGetValue(DashboardKey, out DashboardDto? cached) && cached is not null)
        {
            Interlocked.Increment(ref _hits);
            return cached;
        }

        Interlocked.Increment(ref _misses);

        try
        {
            var dashboard = await _reportRepository.GetDashboardAsync();
            _memoryCache.Set(DashboardKey, dashboard, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = DashboardTtl,
                Size = 1
            });
            _staticCache[DashboardKey] = dashboard;
            return dashboard;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load dashboard data");
            throw;
        }
    }

    public void InvalidateRates()
    {
        _memoryCache.Remove(RatesKey);
        _staticCache.TryRemove(RatesKey, out _);
    }

    public void InvalidateProducts()
    {
        _memoryCache.Remove(ProductsKey);
        _staticCache.TryRemove(ProductsKey, out _);
    }

    public void InvalidateDashboard()
    {
        _memoryCache.Remove(DashboardKey);
        _staticCache.TryRemove(DashboardKey, out _);
    }

    public void InvalidateAll()
    {
        InvalidateRates();
        InvalidateProducts();
        InvalidateDashboard();
    }
}
