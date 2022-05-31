namespace PgCompletionist;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public partial class MainWindow
{
    private const string SettingsName = "Completionist";

    private async Task LoadSettings()
    {
        Settings? Settings = null;

        try
        {
            Settings = await LocalStorage.GetItemAsync<Settings>(SettingsName);
        }
        catch
        {
        }

        if (Settings is not null)
        {
            List<ObservableCharacter> NewList = new();
            foreach (Character Character in Settings.CharacterList)
                NewList.Add(new ObservableCharacter(Character));

            CharacterList.ReplaceRange(NewList);
            int Index = Settings.SelectedCharacterIndex;
            SelectedCharacterIndex = Index >= 0 && Index < CharacterList.Count ? Index : -1;
        }
    }

    private async Task SaveSettings()
    {
        Settings Settings = new();
        foreach (ObservableCharacter Character in CharacterList)
            Settings.CharacterList.Add(Character.Item);
        Settings.SelectedCharacterIndex = SelectedCharacterIndex;

        await LocalStorage.SetItemAsync<Settings>(SettingsName, Settings);
    }
}
