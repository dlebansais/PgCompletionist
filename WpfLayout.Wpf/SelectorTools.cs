namespace WpfLayout;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

public static class SelectorTools
{
    private static FrameworkElement UnusedFrameworkElement = new FrameworkElement();
    private static Panel UnusedPanel = new StackPanel();
    private static ItemsControl UnusedItemsControl = new ItemsControl();

    public static bool FindChildByName(FrameworkElement root, string name, out FrameworkElement child)
    {
        if (root.Name == name)
        {
            child = root;
            return true;
        }

        int Count = VisualTreeHelper.GetChildrenCount(root);

        for (int i = 0; i < Count; i++)
            if (VisualTreeHelper.GetChild(root, i) is FrameworkElement ChildElement && FindChildByName(ChildElement, name, out child))
                return true;

        child = UnusedFrameworkElement;
        return false;
    }

    public static bool FindMenuOwner(RoutedEventArgs e, out FrameworkElement owner)
    {
        object Source = e.OriginalSource;

        while (Source is MenuItem AsMenuItem)
            Source = AsMenuItem.Parent;

        if (Source is ContextMenu AsContextMenu)
        {
            if (AsContextMenu.PlacementTarget is FrameworkElement AsPlacementTarget)
            {
                owner = AsPlacementTarget;
                return true;
            }

            FrameworkElement Root = Application.Current.MainWindow;
            Point MenuClickPoint = AsContextMenu.TranslatePoint(new Point(AsContextMenu.ActualWidth, 0), Root);
            if (Root.InputHitTest(MenuClickPoint) is FrameworkElement AsClickTarget)
            {
                owner = AsClickTarget;
                return true;
            }
        }

        owner = UnusedFrameworkElement;
        return false;
    }

    public static bool FindRootInPanel(DependencyObject element, out Panel panel, out int index)
    {
        while (element is UIElement AsChild)
        {
            DependencyObject Parent = VisualTreeHelper.GetParent(element);

            if (Parent is Panel AsPanel)
            {
                panel = AsPanel;
                index = AsPanel.Children.IndexOf(AsChild);
                return true;
            }

            element = Parent;
        }

        panel = UnusedPanel;
        index = -1;
        return false;
    }

    public static bool FindItemOwnerControl(DependencyObject item, out ItemsControl control)
    {
        DependencyObject Parent = VisualTreeHelper.GetParent(item);

        while (Parent is UIElement)
        {
            if (Parent is ItemsControl AsItemsControl)
            {
                control = AsItemsControl;
                return true;
            }

            if (LogicalTreeHelper.GetParent(Parent) is Popup AsPopup)
                Parent = AsPopup;
            else
                Parent = VisualTreeHelper.GetParent(Parent);
        }

        control = UnusedItemsControl;
        return false;
    }

    public static bool FindPanelChild(FrameworkElement root, out Panel panel)
    {
        if (root is Panel AsPanel)
        {
            panel = AsPanel;
            return true;
        }

        int Count = VisualTreeHelper.GetChildrenCount(root);

        for (int i = 0; i < Count; i++)
            if (VisualTreeHelper.GetChild(root, i) is FrameworkElement ChildElement && FindPanelChild(ChildElement, out panel))
                return true;

        panel = UnusedPanel;
        return false;
    }

    public static DependencyObject? GetParent(DependencyObject dependencyObject)
    {
        return dependencyObject is FrameworkElement Control ? VisualTreeHelper.GetParent(Control) : null;
    }

    public static IInputElement? FocusedElement { get { return Keyboard.FocusedElement; } }

    public static FrameworkElement? ItemToContainer(ItemsControl itemsControl, object item)
    {
        ItemContainerGenerator ItemContainerGenerator = itemsControl.ItemContainerGenerator;
        return ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
    }

    public static double GetGridsplitterPosition(Window root, string name)
    {
        double Value = double.NaN;

        if (FindChildByName(root, name, out FrameworkElement Child) && Child is GridSplitter Control)
        {
            if (Control.Parent is Grid ParentGrid)
            {
                bool IsVertical = Control.VerticalAlignment == VerticalAlignment.Stretch;

                if (IsVertical)
                {
                    int Column = Grid.GetColumn(Control);
                    if (Column > 0 && Column < ParentGrid.ColumnDefinitions.Count)
                    {
                        ColumnDefinition Definition = ParentGrid.ColumnDefinitions[Column - 1];
                        GridLength Length = Definition.Width;

                        if (Length.IsAbsolute)
                            Value = Length.Value;
                    }
                }
                else
                {
                    int Row = Grid.GetRow(Control);
                    if (Row > 0 && Row < ParentGrid.RowDefinitions.Count)
                    {
                        RowDefinition Definition = ParentGrid.RowDefinitions[Row - 1];
                        GridLength Length = Definition.Height;

                        if (Length.IsAbsolute)
                            Value = Length.Value;
                    }
                }
            }
        }

        return Value;
    }

    public static void SetGridsplitterPosition(Window root, string name, double position)
    {
        if (!FindChildByName(root, name, out FrameworkElement Child) || Child is not GridSplitter Control)
            return;

        if (Control.Parent is not Grid ParentGrid)
            return;

        bool IsVertical = Control.VerticalAlignment == VerticalAlignment.Stretch;

        if (IsVertical)
        {
            int Column = Grid.GetColumn(Control);
            if (Column > 0 && Column < ParentGrid.ColumnDefinitions.Count)
            {
                ColumnDefinition Definition = ParentGrid.ColumnDefinitions[Column - 1];
                Definition.Width = new GridLength(position, GridUnitType.Pixel);
            }
        }
        else
        {
            int Row = Grid.GetRow(Control);
            if (Row > 0 && Row < ParentGrid.RowDefinitions.Count)
            {
                RowDefinition Definition = ParentGrid.RowDefinitions[Row - 1];
                Definition.Height = new GridLength(position, GridUnitType.Pixel);
            }
        }
    }

    public static void GetWindowSize(Window root, out double width, out double height)
    {
        width = root.Width;
        height = root.Height;
    }

    public static void SetWindowSize(Window root, double width, double height)
    {
        if (!double.IsNaN(width) && !double.IsNaN(height))
        {
            root.Width = width;
            root.Height = height;
        }
    }
}
