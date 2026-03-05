using Desktopcafe.Core.DTOs;
using Desktopcafe.Core.Enums;

namespace Desktopcafe.Core.Interfaces;

public interface IComputerService
{
    Task<List<ComputerStatusDto>> GetAllComputersAsync();
    Task<ComputerStatusDto?> GetComputerAsync(int id);
    Task UpdateStatusAsync(int computerId, ComputerStatus status, string? notes = null);
    Task RegisterComputerAsync(ComputerRegistrationDto dto);
    Task UpdateHeartbeatAsync(HeartbeatDto dto);
}
