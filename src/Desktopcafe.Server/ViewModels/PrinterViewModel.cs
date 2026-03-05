using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Core.DTOs;
using Desktopcafe.Core.Enums;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Server.ViewModels;

public partial class PrinterViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    public PrinterViewModel(AppDbContext db)
    {
        _db = db;
    }

    public ObservableCollection<PrintJobDto> PendingJobs { get; } = new();

    public ObservableCollection<PrintJobDto> CompletedJobs { get; } = new();

    [RelayCommand]
    private async Task ApproveJobAsync(int jobId)
    {
        try
        {
            var job = await _db.PrintJobs.FindAsync(jobId);
            if (job is null) return;

            job.Status = PrintJobStatus.Approved;
            _db.PrintJobs.Update(job);
            await _db.SaveChangesAsync();
            await RefreshAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private async Task RejectJobAsync(int jobId)
    {
        try
        {
            var job = await _db.PrintJobs.FindAsync(jobId);
            if (job is null) return;

            job.Status = PrintJobStatus.Rejected;
            _db.PrintJobs.Update(job);
            await _db.SaveChangesAsync();
            await RefreshAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            var pending = await _db.PrintJobs
                .Include(j => j.Computer)
                .Where(j => j.Status == PrintJobStatus.Pending)
                .OrderByDescending(j => j.CreatedAt)
                .Select(j => new PrintJobDto(
                    j.Id,
                    j.ComputerId,
                    j.Computer.Name,
                    j.SessionId,
                    j.Pages,
                    j.IsColor,
                    j.Cost,
                    j.Status,
                    j.DocumentName,
                    j.PrinterName,
                    j.CreatedAt))
                .ToListAsync();

            PendingJobs.Clear();
            foreach (var job in pending)
            {
                PendingJobs.Add(job);
            }

            var completed = await _db.PrintJobs
                .Include(j => j.Computer)
                .Where(j => j.Status != PrintJobStatus.Pending)
                .OrderByDescending(j => j.CreatedAt)
                .Take(50)
                .Select(j => new PrintJobDto(
                    j.Id,
                    j.ComputerId,
                    j.Computer.Name,
                    j.SessionId,
                    j.Pages,
                    j.IsColor,
                    j.Cost,
                    j.Status,
                    j.DocumentName,
                    j.PrinterName,
                    j.CreatedAt))
                .ToListAsync();

            CompletedJobs.Clear();
            foreach (var job in completed)
            {
                CompletedJobs.Add(job);
            }
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }
}
