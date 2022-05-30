﻿namespace PgCompletionist;

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
            CharacterList.ReplaceRange(Settings.CharacterList);
            int Index = Settings.SelectedCharacterIndex;
            SelectedCharacterIndex = Index >= 0 && Index < CharacterList.Count ? Index : -1;
        }
    }

    private async Task SaveSettings()
    {
        Settings Settings = new();
        Settings.CharacterList = new(CharacterList);
        Settings.SelectedCharacterIndex = SelectedCharacterIndex;

        await LocalStorage.SetItemAsync<Settings>(SettingsName, Settings);
    }
}
