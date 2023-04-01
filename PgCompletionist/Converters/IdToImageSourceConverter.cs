namespace Converters;

using System;
using System.Globalization;
using System.Windows.Data;
using PgIcons;
using PgObjects;

[ValueConversion(typeof(int), typeof(object))]
public class IdToImageSourceConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int IconId && IconId > 0)
            return IconTools.LoadIcon(Groups.Version, IconId)!;
        else
            return IconTools.LoadEmptyIcon();
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return BindingOperations.DisconnectedSource;
    }
}
