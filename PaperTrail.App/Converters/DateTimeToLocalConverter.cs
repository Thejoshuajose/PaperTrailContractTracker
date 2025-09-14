using System;
using System.Globalization;
using System.Windows.Data;

namespace PaperTrail.App.Converters;

public class DateTimeToLocalConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is DateTime dt ? dt.ToLocalTime() : null;
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is DateTime dt ? dt.ToUniversalTime() : null;
    }
}
