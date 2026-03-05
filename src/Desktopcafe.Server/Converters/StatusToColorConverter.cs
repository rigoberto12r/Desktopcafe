using Desktopcafe.Core.Enums;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Desktopcafe.Server.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var color = value switch
        {
            ComputerStatus.Available => Color.FromArgb(255, 15, 123, 15),
            ComputerStatus.InUse => Color.FromArgb(255, 0, 120, 212),
            ComputerStatus.Maintenance => Color.FromArgb(255, 118, 118, 118),
            ComputerStatus.Offline => Color.FromArgb(255, 68, 68, 68),
            "Success" or "Available" => Color.FromArgb(255, 15, 123, 15),
            "Warning" => Color.FromArgb(255, 157, 93, 0),
            "Error" or "Expiring" => Color.FromArgb(255, 196, 43, 28),
            _ => Color.FromArgb(255, 128, 128, 128)
        };

        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

public class TimeSpanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is TimeSpan ts
            ? ts.TotalHours >= 1
                ? ts.ToString(@"h\:mm\:ss")
                : ts.ToString(@"mm\:ss")
            : "--:--";

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is decimal d ? $"${d:F2}" : "$0.00";

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is true
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
