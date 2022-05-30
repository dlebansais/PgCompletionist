namespace WpfLayout;

using System.Windows;
using System.Windows.Input;

public static class LeftClickContextMenu
{
    private static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(LeftClickContextMenu), new PropertyMetadata(new PropertyChangedCallback(OnIsEnabledChanged)));

    public static bool GetIsEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(IsEnabledProperty, value);
    }

    private static void OnIsEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is FrameworkElement AsFrameworkElement)
        {
            AsFrameworkElement.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            AsFrameworkElement.MouseLeftButtonDown += OnMouseLeftButtonDown;
        }
    }

    private static void OnMouseLeftButtonDown(object sender, MouseEventArgs args)
    {
        if (sender is FrameworkElement AsFrameworkElement && AsFrameworkElement.ContextMenu is not null && GetIsEnabled(AsFrameworkElement))
        {
            AsFrameworkElement.ContextMenu.IsOpen = true;
            args.Handled = true;
        }
    }
}
