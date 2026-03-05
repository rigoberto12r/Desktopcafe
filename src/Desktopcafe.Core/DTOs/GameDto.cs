namespace Desktopcafe.Core.DTOs;

public record GameDto(
    int Id,
    string Name,
    string Category,
    string ExePath,
    string? CoverPath,
    int TimesPlayed);
