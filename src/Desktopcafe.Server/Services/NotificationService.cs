using System.Collections.ObjectModel;

namespace Desktopcafe.Server.Services;

public enum NotificationSeverity
{
    Info,
    Success,
    Warning,
    Error
}

public record Notification(string Title, string Message, NotificationSeverity Severity, DateTime Timestamp)
{
    public Guid Id { get; init; } = Guid.NewGuid();
}

public class NotificationService
{
    public ObservableCollection<Notification> Notifications { get; } = new();

    public void Show(string title, string message, NotificationSeverity severity = NotificationSeverity.Info)
    {
        var notification = new Notification(title, message, severity, DateTime.Now);
        Notifications.Add(notification);
    }

    public void ShowInfo(string title, string message) =>
        Show(title, message, NotificationSeverity.Info);

    public void ShowSuccess(string title, string message) =>
        Show(title, message, NotificationSeverity.Success);

    public void ShowWarning(string title, string message) =>
        Show(title, message, NotificationSeverity.Warning);

    public void ShowError(string title, string message) =>
        Show(title, message, NotificationSeverity.Error);

    public void Dismiss(Guid notificationId)
    {
        var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification is not null)
        {
            Notifications.Remove(notification);
        }
    }

    public void DismissAll() => Notifications.Clear();
}
