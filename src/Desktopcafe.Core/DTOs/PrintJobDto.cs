using Desktopcafe.Core.Enums;

namespace Desktopcafe.Core.DTOs;

public record PrintJobDto(
    int Id,
    int ComputerId,
    string ComputerName,
    int? SessionId,
    int Pages,
    bool IsColor,
    decimal Cost,
    PrintJobStatus Status,
    string DocumentName,
    string PrinterName,
    DateTime CreatedAt);
