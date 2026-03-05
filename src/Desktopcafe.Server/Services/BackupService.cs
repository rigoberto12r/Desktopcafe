using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Desktopcafe.Server.Services;

public class BackupService : BackgroundService
{
    private static readonly TimeSpan BackupInterval = TimeSpan.FromHours(6);
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(7);

    private readonly string _dbPath;
    private readonly string _backupDir;
    private readonly ILogger<BackupService> _logger;

    public BackupService(ILogger<BackupService> logger)
    {
        _logger = logger;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _dbPath = Path.Combine(appData, "Desktopcafe", "desktopcafe.db");
        _backupDir = Path.Combine(appData, "Desktopcafe", "Backups");
        Directory.CreateDirectory(_backupDir);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Backup service started. Interval: {Hours}h, Retention: {Days}d",
            BackupInterval.TotalHours, RetentionPeriod.TotalDays);

        using var timer = new PeriodicTimer(BackupInterval);

        // Run an initial backup on startup
        await PerformBackup(stoppingToken);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PerformBackup(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Backup service stopping");
        }
    }

    public Task BackupNowAsync() => PerformBackup(CancellationToken.None);

    private async Task PerformBackup(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(_dbPath))
            {
                _logger.LogWarning("Database file not found at {Path}, skipping backup", _dbPath);
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"desktopcafe_backup_{timestamp}.db";
            var backupPath = Path.Combine(_backupDir, backupFileName);

            _logger.LogInformation("Creating backup: {FileName}", backupFileName);

            await using (var source = new FileStream(_dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            await using (var destination = new FileStream(backupPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await source.CopyToAsync(destination, cancellationToken);
            }

            var fileInfo = new FileInfo(backupPath);
            _logger.LogInformation("Backup created: {FileName} ({Size:N0} bytes)", backupFileName, fileInfo.Length);

            CleanOldBackups();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to create backup");
        }
    }

    private void CleanOldBackups()
    {
        try
        {
            var cutoff = DateTime.Now - RetentionPeriod;
            var backupFiles = Directory.GetFiles(_backupDir, "desktopcafe_backup_*.db");

            var deleted = 0;
            foreach (var file in backupFiles)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoff)
                {
                    fileInfo.Delete();
                    deleted++;
                }
            }

            if (deleted > 0)
            {
                _logger.LogInformation("Cleaned {Count} old backup(s)", deleted);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean old backups");
        }
    }
}
