namespace Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using WpfLayout;

    [ValueConversion(typeof(int), typeof(object))]
    public class IdToImageSourceConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == BindingOperations.DisconnectedSource)
                return ImageConversion.DefaultImage;

            int IconId = (int)value;
            if (IconId == 0)
                return ImageConversion.DefaultImage;

            return ImageConversion.IconFileToImageSource(IconTools.IdToFile((int)value));
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return BindingOperations.DisconnectedSource;
        }
    }
}
