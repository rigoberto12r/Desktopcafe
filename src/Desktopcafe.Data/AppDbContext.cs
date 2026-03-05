using Desktopcafe.Core.Models;
using Desktopcafe.Data.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Data;

public class AppDbContext : DbContext
{
    private readonly string _dbPath;

    public AppDbContext()
    {
        _dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Desktopcafe", "desktopcafe.db");
        Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Computer> Computers => Set<Computer>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<PrintJob> PrintJobs => Set<PrintJob>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<WiFiVoucher> WiFiVouchers => Set<WiFiVoucher>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RateConfig> RateConfigs => Set<RateConfig>();
    public DbSet<AppConfig> AppConfigs => Set<AppConfig>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={_dbPath};Cache=Shared");
        }
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ComputerConfiguration());
        modelBuilder.ApplyConfiguration(new SessionConfiguration());
        modelBuilder.ApplyConfiguration(new ClientConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new SaleConfiguration());
        modelBuilder.ApplyConfiguration(new SaleItemConfiguration());
        modelBuilder.ApplyConfiguration(new PrintJobConfiguration());
        modelBuilder.ApplyConfiguration(new ShiftConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationConfiguration());
        modelBuilder.ApplyConfiguration(new WiFiVoucherConfiguration());
        modelBuilder.ApplyConfiguration(new GameConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new RateConfigConfiguration());
        modelBuilder.ApplyConfiguration(new AppConfigConfiguration());
    }

    public async Task ApplyPragmasAsync()
    {
        var connection = Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            PRAGMA journal_mode = WAL;
            PRAGMA synchronous = NORMAL;
            PRAGMA cache_size = -20000;
            PRAGMA temp_store = MEMORY;
            PRAGMA mmap_size = 268435456;
            PRAGMA busy_timeout = 5000;
        ";
        await cmd.ExecuteNonQueryAsync();
    }
}
