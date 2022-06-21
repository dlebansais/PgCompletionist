namespace PgCompletionist;

using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
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
            TaskDispatcher.Dispatch(() => ExecuteAddReport(Result.FilePath, Result.ContentBytes));
        }
    }

    private async Task ExecuteAddReport(string fileName, byte[] contentBytes)
    {
        await Task.Run(() => ParseReportFile(fileName, contentBytes));
        SetIsAnalyzing(false);
    }

    public override void OnDelete(object sender, ExecutedRoutedEventArgs args)
    {
        if (args.OriginalSource is FrameworkElement Source && Source.DataContext is ObservableCharacter AsCharacter && CharacterList.Contains(AsCharacter))
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

    public override async void OnExpand(object sender, ExecutedRoutedEventArgs args)
    {
        if (args.OriginalSource is Hyperlink Source && Source.DataContext is IMoreToSee AsItem && CurrentCharacter is ObservableCharacter AsCharacter)
        {
            int MaxCount = int.MaxValue;

            if (AsItem.MoreToSee >= ExpandTools.PerfLimit)
            {
                MessageBoxResult WarningResult = await DialogBox.Show("This is going to take like... forever. Are you sure?", "Large display delay", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (WarningResult != MessageBoxResult.OK)
                    return;
            }

            switch (AsItem)
            {
                case MissingSkill:
                    AsCharacter.ExpandMissingSkills(MaxCount);
                    break;
                case NonMaxedSkill:
                    AsCharacter.ExpandNonMaxedSkills(MaxCount);
                    break;
                case ObservableMissingAbilitesBySkill:
                    AsCharacter.ExpandMissingAbilitiesList(MaxCount);
                    break;
                case MissingAbility AsMissingAbility:
                    AsCharacter.ExpandMissingAbility(AsMissingAbility, MaxCount);
                    break;
                case MissingRecipe:
                    AsCharacter.ExpandMissingRecipes(MaxCount);
                    break;
                case NeverEatenFood:
                    AsCharacter.ExpandNeverEatenFoods(MaxCount);
                    break;
            }
        }
    }

    public override void OnAddGourmand(object sender, ExecutedRoutedEventArgs args)
    {
        FileDialogResult Result = (FileDialogResult)args.Parameter;
        if (Result.FilePath.Length > 0)
        {
            SetIsAnalyzing(true);
            TaskDispatcher.Dispatch(() => ExecuteAddGourmand(Result.FilePath, Result.ContentBytes));
        }
    }

    private async Task ExecuteAddGourmand(string fileName, byte[] contentBytes)
    {
        await Task.Run(() => ParseGourmandFile(fileName, contentBytes));
        SetIsAnalyzing(false);
    }
}
