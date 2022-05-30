namespace WpfLayout;

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public static class DragDropTools
{
    #region Events
    public static void OnListBoxItemMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && sender is ListBoxItem)
        {
            ListBoxItem DraggedItem = (ListBoxItem)sender;
            DragDrop.DoDragDrop(DraggedItem, DraggedItem.DataContext, DragDropEffects.Move);
            DraggedItem.IsSelected = true;
        }
    }

    public static void OnListBoxItemDrop(object sender, DragEventArgs e)
    {
        object Target = ((ListBoxItem)sender).DataContext;
        object Dropped = e.Data.GetData(Target.GetType());

        DependencyObject Ancestor = (DependencyObject)sender;
        while (Ancestor != null && !(Ancestor is ListBox))
            Ancestor = VisualTreeHelper.GetParent(Ancestor);

        if (Ancestor != null)
        {
            ListBox container = (ListBox)Ancestor;

            int RemoveIndex = container.Items.IndexOf(Dropped);
            int TargetIndex = container.Items.IndexOf(Target);

            IList ItemsSource = (IList)container.ItemsSource;

            if (RemoveIndex < TargetIndex)
            {
                ItemsSource.Insert(TargetIndex + 1, Dropped);
                ItemsSource.RemoveAt(RemoveIndex);
            }
            else if (ItemsSource.Count > RemoveIndex)
            {
                ItemsSource.Insert(TargetIndex, Dropped);
                ItemsSource.RemoveAt(RemoveIndex + 1);
            }
        }
    }
    #endregion
}
