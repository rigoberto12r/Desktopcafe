using Desktopcafe.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Desktopcafe.Data.Configuration;

public class PrintJobConfiguration : IEntityTypeConfiguration<PrintJob>
{
    public void Configure(EntityTypeBuilder<PrintJob> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Cost).HasColumnType("decimal(10,2)");
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.DocumentName).HasMaxLength(260);
        builder.Property(p => p.PrinterName).HasMaxLength(100);

        builder.HasOne(p => p.Session).WithMany(s => s.PrintJobs).HasForeignKey(p => p.SessionId);
        builder.HasOne(p => p.Computer).WithMany(c => c.PrintJobs).HasForeignKey(p => p.ComputerId);

        builder.HasIndex(p => p.CreatedAt).HasDatabaseName("IX_PrintJobs_Date");
    }
}

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.CashStart).HasColumnType("decimal(10,2)");
        builder.Property(s => s.CashEnd).HasColumnType("decimal(10,2)");
        builder.Property(s => s.Notes).HasMaxLength(500);
        builder.HasOne(s => s.Employee).WithMany(e => e.Shifts).HasForeignKey(s => s.EmployeeId);
    }
}

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Notes).HasMaxLength(500);
        builder.HasOne(r => r.Computer).WithMany(c => c.Reservations).HasForeignKey(r => r.ComputerId);
        builder.HasOne(r => r.Client).WithMany(c => c.Reservations).HasForeignKey(r => r.ClientId);
    }
}

public class WiFiVoucherConfiguration : IEntityTypeConfiguration<WiFiVoucher>
{
    public void Configure(EntityTypeBuilder<WiFiVoucher> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Code).HasMaxLength(10).IsRequired();
        builder.Property(v => v.MacAddress).HasMaxLength(17);
        builder.HasIndex(v => v.Code).IsUnique().HasDatabaseName("IX_WiFiVouchers_Code");
    }
}

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
        builder.Property(g => g.Category).HasMaxLength(50);
        builder.Property(g => g.ExePath).HasMaxLength(500);
        builder.Property(g => g.CoverPath).HasMaxLength(500);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Details).HasMaxLength(2000);
        builder.HasOne(a => a.Employee).WithMany(e => e.AuditLogs).HasForeignKey(a => a.EmployeeId);
        builder.HasIndex(a => a.CreatedAt).HasDatabaseName("IX_AuditLogs_Date");
    }
}

public class RateConfigConfiguration : IEntityTypeConfiguration<RateConfig>
{
    public void Configure(EntityTypeBuilder<RateConfig> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).HasMaxLength(50).IsRequired();
        builder.Property(r => r.PricePerHour).HasColumnType("decimal(10,2)");
        builder.Property(r => r.PricePerHalfHour).HasColumnType("decimal(10,2)");
        builder.Property(r => r.PricePer15Min).HasColumnType("decimal(10,2)");
    }
}

public class AppConfigConfiguration : IEntityTypeConfiguration<AppConfig>
{
    public void Configure(EntityTypeBuilder<AppConfig> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Key).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Value).HasMaxLength(500);
        builder.Property(c => c.Description).HasMaxLength(200);
        builder.HasIndex(c => c.Key).IsUnique().HasDatabaseName("IX_AppConfig_Key");
    }
}
