namespace WpfLayout;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
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

    public static bool ParseEventArguments<TDataContext>(RoutedEventArgs args, out TDataContext dataContext)
    {
        if (args.OriginalSource is Hyperlink HyperlinkSource)
        {
            if (HyperlinkSource.DataContext is TDataContext AsDataContext)
            {
                dataContext = AsDataContext;
                return true;
            }
        }

        if (args.OriginalSource is FrameworkElement ElementSource)
        {
            if (ElementSource.DataContext is TDataContext AsDataContext)
            {
                dataContext = AsDataContext;
                return true;
            }
        }

        dataContext = default!;
        return false;
    }

    public static bool ParseEventArguments<TControl, TDataContext>(RoutedEventArgs args, out TControl control, out TDataContext dataContext)
    {
        if (args.OriginalSource is FrameworkElement Source && Source is TControl AsControl)
        {
            if (Source.DataContext is TDataContext AsDataContext)
            {
                control = AsControl;
                dataContext = AsDataContext;
                return true;
            }
        }

        control = default!;
        dataContext = default!;
        return false;
    }

    public static GridsplitterPosition GetGridsplitterPosition(Window root, string name)
    {
        GridsplitterPosition Value = new();

        if (FindChildByName(root, name, out FrameworkElement Child) && Child is GridSplitter Control)
        {
            if (Control.Parent is Grid ParentGrid)
            {
                bool IsVertical = Control.VerticalAlignment == VerticalAlignment.Stretch;

                if (IsVertical)
                {
                    int Column = Grid.GetColumn(Control);
                    if (Column > 0 && Column + 1 < ParentGrid.ColumnDefinitions.Count)
                    {
                        ColumnDefinition DefinitionBefore = ParentGrid.ColumnDefinitions[Column - 1];
                        GridLength LengthBefore = DefinitionBefore.Width;
                        ColumnDefinition DefinitionAfter = ParentGrid.ColumnDefinitions[Column + 1];
                        GridLength LengthAfter = DefinitionAfter.Width;

                        Value = new() { Before = LengthBefore.Value, After = LengthAfter.Value };
                    }
                }
                else
                {
                    int Row = Grid.GetRow(Control);
                    if (Row > 0 && Row + 1 < ParentGrid.RowDefinitions.Count)
                    {
                        RowDefinition DefinitionBefore = ParentGrid.RowDefinitions[Row - 1];
                        GridLength LengthBefore = DefinitionBefore.Height;
                        RowDefinition DefinitionAfter = ParentGrid.RowDefinitions[Row + 1];
                        GridLength LengthAfter = DefinitionAfter.Height;

                        Value = new() { Before = LengthBefore.Value, After = LengthAfter.Value };
                    }
                }
            }
        }

        return Value;
    }

    public static void SetGridsplitterPosition(Window root, string name, GridsplitterPosition position)
    {
        if (!FindChildByName(root, name, out FrameworkElement Child) || Child is not GridSplitter Control || double.IsNaN(position.Before) || double.IsNaN(position.After))
            return;

        if (Control.Parent is not Grid ParentGrid)
            return;

        bool IsVertical = Control.VerticalAlignment == VerticalAlignment.Stretch;

        if (IsVertical)
        {
            int Column = Grid.GetColumn(Control);
            if (Column > 0 && Column + 1 < ParentGrid.ColumnDefinitions.Count)
            {
                ColumnDefinition DefinitionBefore = ParentGrid.ColumnDefinitions[Column - 1];
                GridLength LengthBefore = DefinitionBefore.Width;
                ColumnDefinition DefinitionAfter = ParentGrid.ColumnDefinitions[Column + 1];
                GridLength LengthAfter = DefinitionAfter.Width;

                DefinitionBefore.Width = new GridLength(position.Before, LengthBefore.GridUnitType);
                DefinitionAfter.Width = new GridLength(position.After, LengthAfter.GridUnitType);
            }
        }
        else
        {
            int Row = Grid.GetRow(Control);
            if (Row > 0 && Row + 1 < ParentGrid.RowDefinitions.Count)
            {
                RowDefinition DefinitionBefore = ParentGrid.RowDefinitions[Row - 1];
                GridLength LengthBefore = DefinitionBefore.Height;
                RowDefinition DefinitionAfter = ParentGrid.RowDefinitions[Row + 1];
                GridLength LengthAfter = DefinitionAfter.Height;

                DefinitionBefore.Height = new GridLength(position.Before, LengthBefore.GridUnitType);
                DefinitionAfter.Height = new GridLength(position.After, LengthAfter.GridUnitType);
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
