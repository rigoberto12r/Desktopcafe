using CommunityToolkit.Mvvm.ComponentModel;

namespace Desktopcafe.Agent.ViewModels;

public partial class SessionOverlayViewModel : ObservableObject
{
    [ObservableProperty] private string _computerName = "";
    [ObservableProperty] private string _clientName = "";
    [ObservableProperty] private TimeSpan _timeRemaining;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private decimal _totalCharge;
    [ObservableProperty] private int _printPages;
    [ObservableProperty] private decimal _printCost;
    [ObservableProperty] private bool _isExpanded;
}
