using Desktopcafe.Core.Enums;
using Desktopcafe.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Data.Repositories;

public class SessionRepository : Repository<Session>
{
    public SessionRepository(AppDbContext db) : base(db) { }

    public async Task<List<Session>> GetActiveSessionsAsync() =>
        await Db.Sessions
            .Include(s => s.Computer)
            .Include(s => s.Client)
            .Where(s => s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused)
            .OrderBy(s => s.PlannedEnd)
            .ToListAsync();

    public async Task<Session?> GetActiveSessionForComputerAsync(int computerId) =>
        await Db.Sessions
            .Include(s => s.Computer)
            .Include(s => s.Client)
            .FirstOrDefaultAsync(s =>
                s.ComputerId == computerId &&
                (s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused));

    public async Task<List<Session>> GetSessionHistoryAsync(DateTime from, DateTime to) =>
        await Db.Sessions
            .Include(s => s.Computer)
            .Include(s => s.Client)
            .Include(s => s.Employee)
            .Where(s => s.CreatedAt >= from && s.CreatedAt < to.AddDays(1))
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

    public async Task<List<Session>> GetClientSessionsAsync(int clientId) =>
        await Db.Sessions
            .Include(s => s.Computer)
            .Where(s => s.ClientId == clientId)
            .OrderByDescending(s => s.CreatedAt)
            .Take(50)
            .ToListAsync();

    public async Task<int> GetActiveCountAsync() =>
        await Db.Sessions.CountAsync(s => s.Status == SessionStatus.Active);
}
