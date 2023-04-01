namespace PgCompletionist;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public partial class MainWindow
{
    private async Task LoadSettings()
    {
        SetSettingsPrefix();

        try
        {
            if (await LocalStorage.GetItemAsync<Settings>($"{SettingsPrefix}\\{typeof(Settings).Name}") is Settings NewSettings)
            {
                Settings = NewSettings;

                List<ObservableCharacter> NewList = new();
                foreach (Character Character in Settings.CharacterList)
                    NewList.Add(new ObservableCharacter(Character));

                NewList.Sort(SortByName);
                CharacterList.ReplaceRange(NewList);

                int Index = CharacterList.Count > 0 ? 0 : -1;
                if (Settings.SelectedCharacter is string SelectedCharacter)
                {
                    for (int i = 0; i < CharacterList.Count; i++)
                        if (CharacterList[i].Name == SelectedCharacter)
                        {
                            Index = i;
                            break;
                        }
                }

                TaskDispatcher.Dispatch(() => { SelectedCharacterIndex = Index; });
            }
        }
        catch
        {
        }
    }

    private void SetSettingsPrefix()
    {
        if (SettingsPrefix.Length > 0)
            return;

        Type SettingsType = Settings.GetType();
        SettingsPrefix = SettingsType.FullName!.Replace(".", "\\").Split('\\')[0];
    }

    private string SettingsPrefix = string.Empty;
    public Settings Settings { get; private set; } = new Settings();

    private static int SortByName(ObservableCharacter item1, ObservableCharacter item2)
    {
        return string.Compare(item1.Name, item2.Name, StringComparison.InvariantCultureIgnoreCase);
    }

    private async Task SaveSettings()
    {
        SetSettingsPrefix();

        Settings = new();
        foreach (ObservableCharacter Character in CharacterList)
            Settings.CharacterList.Add(Character.Item);

        Settings.SelectedCharacter = (CurrentCharacter as ObservableCharacter)?.Name;

        await LocalStorage.SetItemAsync($"{SettingsPrefix}\\{typeof(Settings).Name}", Settings);
    }
}
