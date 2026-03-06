using Desktopcafe.Core.Enums;
using Desktopcafe.Core.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Desktopcafe.Data.Repositories;

public class SessionRepository : Repository<Session>
{
    public SessionRepository(AppDbContext db) : base(db) { }

    public async Task<List<Session>> GetActiveSessionsAsync()
    {
        try
        {
            return await Db.Sessions
                .Include(s => s.Computer)
                .Include(s => s.Client)
                .Where(s => s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused)
                .OrderBy(s => s.PlannedEnd)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get active sessions");
            throw;
        }
    }

    public async Task<Session?> GetActiveSessionForComputerAsync(int computerId)
    {
        try
        {
            return await Db.Sessions
                .Include(s => s.Computer)
                .Include(s => s.Client)
                .FirstOrDefaultAsync(s =>
                    s.ComputerId == computerId &&
                    (s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get active session for computer {ComputerId}", computerId);
            throw;
        }
    }

    public async Task<List<Session>> GetSessionHistoryAsync(DateTime from, DateTime to)
    {
        try
        {
            return await Db.Sessions
                .Include(s => s.Computer)
                .Include(s => s.Client)
                .Include(s => s.Employee)
                .Where(s => s.CreatedAt >= from && s.CreatedAt < to.AddDays(1))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get session history from {From} to {To}", from, to);
            throw;
        }
    }

    public async Task<List<Session>> GetClientSessionsAsync(int clientId)
    {
        try
        {
            return await Db.Sessions
                .Include(s => s.Computer)
                .Where(s => s.ClientId == clientId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(50)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get sessions for client {ClientId}", clientId);
            throw;
        }
    }

    public async Task<int> GetActiveCountAsync()
    {
        try
        {
            return await Db.Sessions.CountAsync(s => s.Status == SessionStatus.Active);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get active session count");
            throw;
        }
    }
}
