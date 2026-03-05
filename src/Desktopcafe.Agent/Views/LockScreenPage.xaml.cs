using Microsoft.UI.Xaml.Controls;

namespace Desktopcafe.Agent.Views;

public sealed partial class LockScreenPage : UserControl
{
    private readonly PeriodicTimer _clockTimer = new(TimeSpan.FromSeconds(1));

    public LockScreenPage()
    {
        InitializeComponent();
        _ = UpdateClockAsync();
    }

    private async Task UpdateClockAsync()
    {
        while (true)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ClockText.Text = DateTime.Now.ToString("hh:mm tt");
                DateText.Text = DateTime.Now.ToString("dd/MM/yyyy");
            });

            try { await _clockTimer.WaitForNextTickAsync(); }
            catch { break; }
        }
    }

    public void SetComputerName(string name) => ComputerNameText.Text = name;
    public void SetMessage(string message) => LockMessageText.Text = message;

    public void SetConnectionStatus(bool connected)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            StatusDot.Fill = connected
                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green)
                : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
            StatusText.Text = connected ? "Conectado" : "Desconectado";
        });
    }
}
