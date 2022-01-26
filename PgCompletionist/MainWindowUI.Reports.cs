namespace PgCompletionist;

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public abstract partial class MainWindowUI
{
    protected void InitReports()
    {
        string ChatLogPath = NativeMethods.GetKnownFolderPath(NativeMethods.LocalLowId);
        string LocalLowFolder = Path.Combine(ChatLogPath, @"Elder Game\Project Gorgon");
        ReportFolder = Path.Combine(LocalLowFolder, "Reports");

        ParseReports();
    }

    protected void ParseReports()
    {
        string[] Files = Directory.GetFiles(ReportFolder, "*.json");
        foreach (string File in Files)
            ParseFile(File);
    }

    protected void ParseFile(string fileName)
    {
        string FileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        if (FileNameWithoutExtension.StartsWith("Character_"))
            ParseCharacterReport(fileName, FileNameWithoutExtension.Substring(10));
    }

    protected void ParseCharacterReport(string fileName, string characterName)
    {
        using FileStream Stream = new(fileName, FileMode.Open, FileAccess.Read);

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
                AddCharacter(NewCharacter);
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

    private string ReportFolder = string.Empty;
}
