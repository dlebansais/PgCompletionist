namespace PgCompletionist;

using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class MainWindow
{
    private void ParseFile(string fileName, string content)
    {
        string FileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        if (FileNameWithoutExtension.StartsWith("Character_"))
            ParseCharacterReport(FileNameWithoutExtension.Substring(10), content);
    }

    private void ParseCharacterReport(string characterName, string content)
    {
        byte[] ContentBytes = Encoding.UTF8.GetBytes(content);
        using MemoryStream Stream = new(ContentBytes);

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
}
