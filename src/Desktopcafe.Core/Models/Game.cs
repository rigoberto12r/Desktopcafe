namespace Desktopcafe.Core.Models;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ExePath { get; set; } = string.Empty;
    public string? CoverPath { get; set; }
    public int TimesPlayed { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
