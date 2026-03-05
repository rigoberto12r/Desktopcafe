using System.Diagnostics;
using Desktopcafe.Core.DTOs;
using Serilog;

namespace Desktopcafe.Agent.Services;

public class GameLauncherService
{
    private readonly SignalRClientService _signalR;
    private readonly List<GameDto> _games = new();

    public GameLauncherService(SignalRClientService signalR)
    {
        _signalR = signalR;
        _signalR.GameCatalogUpdated += games =>
        {
            _games.Clear();
            _games.AddRange(games);
        };
    }

    public List<GameDto> GetGames() => _games.ToList();

    public async Task LaunchGameAsync(GameDto game)
    {
        try
        {
            if (!File.Exists(game.ExePath))
            {
                Log.Warning("Game executable not found: {Path}", game.ExePath);
                return;
            }

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = game.ExePath,
                UseShellExecute = true
            });

            if (process != null)
            {
                Log.Information("Launched game: {Name}", game.Name);
                await _signalR.AlertAdminAsync("Juego iniciado", $"{game.Name} en {Environment.MachineName}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to launch game: {Name}", game.Name);
        }
    }
}
