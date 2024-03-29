﻿namespace WpfLayout;

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public static class DragDropTools
{
    public static void Enable(bool isEnabled)
    {
        if (!isEnabled)
            DisableCount++;
        else if (DisableCount > 0)
            DisableCount--;
    }

    private static int DisableCount { get; set; }
    public static bool IsEnabled { get { return DisableCount == 0; } }

    public static void OnListBoxItemMouseMove(object sender, MouseEventArgs args)
    {
        if (!IsEnabled)
            return;

        try
        {
            if (args.LeftButton == MouseButtonState.Pressed && sender is ListBoxItem)
            {
                ListBoxItem DraggedItem = (ListBoxItem)sender;
                DragDrop.DoDragDrop(DraggedItem, DraggedItem.DataContext, DragDropEffects.Move);
                DraggedItem.IsSelected = true;
            }
        }
        catch
        {
        }
    }

    public static void OnListBoxItemDrop(object sender, DragEventArgs args)
    {
        object Target = ((ListBoxItem)sender).DataContext;
        object Dropped = args.Data.GetData(Target.GetType());

        DependencyObject Ancestor = (DependencyObject)sender;
        while (Ancestor != null && !(Ancestor is ListBox))
            Ancestor = VisualTreeHelper.GetParent(Ancestor);

        if (Ancestor != null)
        {
            ListBox container = (ListBox)Ancestor;

            int RemoveIndex = container.Items.IndexOf(Dropped);
            int TargetIndex = container.Items.IndexOf(Target);

            IList ItemsSource = (IList)container.ItemsSource;

            if (RemoveIndex < 0)
            {
                ItemsSource.Insert(TargetIndex, Dropped);
            }
            else if (RemoveIndex < TargetIndex)
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
}
