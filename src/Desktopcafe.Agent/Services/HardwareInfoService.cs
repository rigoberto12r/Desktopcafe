using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Serilog;

namespace Desktopcafe.Agent.Services;

public class HardwareInfoService
{
    private readonly PerformanceCounter? _cpuCounter;

    public HardwareInfoService()
    {
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _cpuCounter.NextValue(); // First call always returns 0
        }
        catch (Exception ex) { Log.Warning(ex, "Failed to initialize CPU counter"); }
    }

    public double GetCpuUsage()
    {
        try { return _cpuCounter?.NextValue() ?? 0; }
        catch (Exception ex) { Log.Warning(ex, "Failed to read CPU usage"); return 0; }
    }

    public double GetRamUsage()
    {
        try
        {
            var info = GC.GetGCMemoryInfo();
            var totalMemory = info.TotalAvailableMemoryBytes;
            var usedMemory = totalMemory - info.MemoryLoadBytes;
            return totalMemory > 0 ? (double)usedMemory / totalMemory * 100 : 0;
        }
        catch (Exception ex) { Log.Warning(ex, "Failed to read RAM usage"); return 0; }
    }

    public double GetDiskUsage()
    {
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory) ?? "C");
            return (1.0 - (double)drive.AvailableFreeSpace / drive.TotalSize) * 100;
        }
        catch (Exception ex) { Log.Warning(ex, "Failed to read disk usage"); return 0; }
    }

    public string GetLocalIpAddress()
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            return (socket.LocalEndPoint as IPEndPoint)?.Address.ToString() ?? "127.0.0.1";
        }
        catch (Exception ex) { Log.Warning(ex, "Failed to get local IP address"); return "127.0.0.1"; }
    }

    public string GetSystemSpecs()
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            MachineName = Environment.MachineName,
            OSVersion = Environment.OSVersion.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            Is64Bit = Environment.Is64BitOperatingSystem
        });
    }
}
