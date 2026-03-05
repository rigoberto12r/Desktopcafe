using Desktopcafe.Core.Enums;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Desktopcafe.Server.Views;

public sealed partial class PrinterPage : Page
{
    public PrinterPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadJobsAsync();
    }

    private async Task LoadJobsAsync()
    {
        using var db = new AppDbContext();
        var today = DateTime.Today;

        var jobs = await db.PrintJobs
            .Include(j => j.Computer)
            .Where(j => j.CreatedAt >= today)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        var pending = jobs.Where(j => j.Status == PrintJobStatus.Pending).Select(MapJob).ToList();
        var completed = jobs.Where(j => j.Status != PrintJobStatus.Pending).Select(MapJob).ToList();

        PendingList.ItemsSource = pending;
        CompletedList.ItemsSource = completed;
    }

    private static object MapJob(PrintJob j) => new
    {
        j.Id,
        ComputerName = j.Computer?.Name ?? "Desconocido",
        j.DocumentName,
        PagesText = $"{j.Pages} pag",
        ColorText = j.IsColor ? "Color" : "B/N",
        CostText = $"${j.Cost:F2}",
        StatusText = j.Status switch
        {
            PrintJobStatus.Pending => "Pendiente",
            PrintJobStatus.Approved => "Aprobado",
            PrintJobStatus.Rejected => "Rechazado",
            PrintJobStatus.Printed => "Impreso",
            PrintJobStatus.Failed => "Fallido",
            _ => j.Status.ToString()
        }
    };

    private async void Approve_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int jobId) return;
        await UpdateJobStatusAsync(jobId, PrintJobStatus.Approved);
    }

    private async void Reject_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int jobId) return;
        await UpdateJobStatusAsync(jobId, PrintJobStatus.Rejected);
    }

    private async Task UpdateJobStatusAsync(int jobId, PrintJobStatus status)
    {
        using var db = new AppDbContext();
        var job = await db.PrintJobs.FindAsync(jobId);
        if (job == null) return;

        job.Status = status;
        db.Entry(job).State = EntityState.Modified;
        await db.SaveChangesAsync();
        await LoadJobsAsync();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadJobsAsync();
}
