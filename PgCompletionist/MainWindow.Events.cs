namespace PgCompletionist;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using PgObjects;
using WpfLayout;

public partial class MainWindow
{
    public override async void OnMainWindowLoaded(object sender, RoutedEventArgs args)
    {
        await LoadSettings();
    }

    public override async void OnMainWindowClosing(object sender, CancelEventArgs args)
    {
        await SaveSettings();
    }

    public override void OnAddReport(object sender, ExecutedRoutedEventArgs args)
    {
        FileDialogResult Result = (FileDialogResult)args.Parameter;
        if (Result.FilePath.Length > 0)
            ParseFile(Result.FilePath);
    }
}
