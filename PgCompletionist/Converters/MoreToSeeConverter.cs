namespace Converters;

using PgCompletionist;
using System;
using System.Globalization;
using System.Windows.Data;

[ValueConversion(typeof(IMoreToSee), typeof(string))]
public class MoreToSeeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IMoreToSee AsMoreToSee)
            if (AsMoreToSee is ObservableMissingAbilitesBySkill)
                return $"See {AsMoreToSee.MoreToSee} more skills with missing abilities";
            else
                return $"See {AsMoreToSee.MoreToSee} more";
        else
            return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return BindingOperations.DisconnectedSource;
    }
}
