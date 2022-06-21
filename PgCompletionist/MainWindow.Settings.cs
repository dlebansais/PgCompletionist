namespace PgCompletionist;

using System;
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

    private static int SortByName(ObservableCharacter item1, ObservableCharacter item2)
    {
        return string.Compare(item1.Name, item2.Name, StringComparison.InvariantCultureIgnoreCase);
    }

    private async Task SaveSettings()
    {
        Settings Settings = new();
        foreach (ObservableCharacter Character in CharacterList)
            Settings.CharacterList.Add(Character.Item);

        Settings.SelectedCharacter = (CurrentCharacter as ObservableCharacter)?.Name;

        await LocalStorage.SetItemAsync<Settings>(SettingsName, Settings);
    }
}
