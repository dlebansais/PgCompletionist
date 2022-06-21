namespace PgCompletionist;

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class MainWindow
{
    private void ParseReportFile(string fileName, byte[] contentBytes)
    {
        string FileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        if (FileNameWithoutExtension.StartsWith("Character_"))
            ParseCharacterReport(FileNameWithoutExtension.Substring(10), contentBytes);
    }

    private void ParseCharacterReport(string characterName, byte[] contentBytes)
    {
        using MemoryStream Stream = new(contentBytes);

        JsonSerializerOptions Options = new()
        {
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            }
        };

        if (JsonSerializer.Deserialize<CharacterReport>(Stream, Options) is CharacterReport Report)
            if (IsValidReport(Report, characterName))
            {
                Character NewCharacter = new Character(Report, characterName);

                TaskDispatcher.Dispatch(() => AddCharacter(NewCharacter));
            }
    }

    private static bool IsValidReport(CharacterReport report, string characterName)
    {
        if (report.Character is not string ReportCharacterName || ReportCharacterName != characterName)
            return false;
        if (report.Report != "CharacterSheet")
            return false;
        if (!report.ReportVersion.HasValue || report.ReportVersion.Value > 1)
            return false;
        if (!report.Race.HasValue)
            return false;

        return true;
    }

    private void AddCharacter(Character newCharacter)
    {
        ObservableCharacter NewItem = new ObservableCharacter(newCharacter);

        for (int i = 0; i < CharacterList.Count; i++)

            if (CharacterList[i].Item.Name == newCharacter.Name)
            {
                CharacterList[i] = NewItem;
                SelectedCharacterIndex = i;
                return;
            }

        SelectedCharacterIndex = -1;

        CharacterList.Add(NewItem);

        SelectedCharacterIndex = CharacterList.Count - 1;
    }

    private void ParseGourmandFile(string fileName, byte[] contentBytes)
    {
        string FileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        string[] FileNameParts = FileNameWithoutExtension.Split('_');
        
        if (FileNameParts.Length == 3)
        {
            string DateString = FileNameParts[1];
            string TimeString = FileNameParts[2];

            if (DateString.Length == 6 && TimeString.Length == 6)
            {
                if ((int.TryParse(DateString.Substring(0, 2), out int Year) && int.TryParse(DateString.Substring(2, 2), out int Month) && int.TryParse(DateString.Substring(4, 2), out int Day)) &&
                    (int.TryParse(TimeString.Substring(0, 2), out int Hour) && int.TryParse(TimeString.Substring(2, 2), out int Minute) && int.TryParse(TimeString.Substring(4, 2), out int Second)))
                {
                    DateTime ReportTime = new DateTime(2000 + Year, Month, Day, Hour, Minute, Second, DateTimeKind.Local);

                    if (CurrentCharacter is ObservableCharacter AsCharacter)
                    {
                        bool IsExpanded = AsCharacter.NeverEatenFoods.Count > 0 && AsCharacter.NeverEatenFoods.Count == AsCharacter.Item.NeverEatenFoods.Count;
                        AsCharacter.Item.UpdateGourmand(ReportTime, contentBytes);
                        TaskDispatcher.Dispatch(() => AsCharacter.UpdateGourmand(IsExpanded));
                    }
                }
            }
        }
    }
}
