using System;
using System.Globalization;
using System.Windows.Data;

namespace PaperTrail.App.Converters;

public class EnumToItemsSourceConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is Type enumType && enumType.IsEnum)
        {
            return Enum.GetValues(enumType);
        }
        if (value is Type type && type.IsEnum)
        {
            return Enum.GetValues(type);
        }
        return Array.Empty<object>();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
