namespace PgCompletionist;

using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
        {
            SetIsAnalyzing(true);
            TaskDispatcher.Dispatch(() => ExecuteAddReport(Result.FilePath, Result.Content));
        }
    }

    private async Task ExecuteAddReport(string fileName, string content)
    {
        await Task.Run(() => ParseFile(fileName, content));
        SetIsAnalyzing(false);
    }

    public override void OnDelete(object sender, ExecutedRoutedEventArgs args)
    {
        if (args.OriginalSource is FrameworkElement Source && Source.DataContext is Character AsCharacter && CharacterList.Contains(AsCharacter))
        {
            int OldIndex = SelectedCharacterIndex;

            SelectedCharacterIndex = -1;
            CharacterList.Remove(AsCharacter);

            if (OldIndex < CharacterList.Count)
                SelectedCharacterIndex = OldIndex;
            else if (CharacterList.Count > 0)
                SelectedCharacterIndex = CharacterList.Count - 1;
        }
    }
}
