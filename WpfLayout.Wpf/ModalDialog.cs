namespace WpfLayout;

using System;
using System.Windows;

public static class ModalDialog
{
    public static bool Show<T>(Window owner, Action<T> getStatus)
        where T : Window, new()
    {
        T Dlg = new();
        Dlg.Owner = owner;
        Dlg.ShowDialog();

        if (!Dlg.DialogResult.HasValue || Dlg.DialogResult.Value == false)
            return false;

        getStatus(Dlg);
        return true;
    }
}
