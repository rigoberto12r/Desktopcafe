using Desktopcafe.Core.Enums;

namespace Desktopcafe.Core.Models;

public class PrintJob
{
    public int Id { get; set; }
    public int? SessionId { get; set; }
    public int ComputerId { get; set; }
    public int Pages { get; set; }
    public bool IsColor { get; set; }
    public decimal Cost { get; set; }
    public PrintJobStatus Status { get; set; } = PrintJobStatus.Pending;
    public string DocumentName { get; set; } = string.Empty;
    public string PrinterName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Session? Session { get; set; }
    public Computer Computer { get; set; } = null!;
}
