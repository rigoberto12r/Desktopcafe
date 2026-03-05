using CommunityToolkit.Mvvm.ComponentModel;

namespace Desktopcafe.Agent.ViewModels;

public partial class LockScreenViewModel : ObservableObject
{
    [ObservableProperty] private string _computerName = Environment.MachineName;
    [ObservableProperty] private string _lockMessage = "Esta computadora esta bloqueada";
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _currentTime = DateTime.Now.ToString("hh:mm tt");
    [ObservableProperty] private string _currentDate = DateTime.Now.ToString("dd/MM/yyyy");
}
