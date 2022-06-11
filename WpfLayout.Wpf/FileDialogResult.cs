namespace WpfLayout;

using System.Collections.Generic;

public record FileDialogResult
{
    public FileDialogResult(FileDialogMode mode, string filePath)
        : this(mode, filePath, string.Empty)
    {
    }

    public FileDialogResult(FileDialogMode mode, string filePath, string content)
    {
        Mode = mode;
        FilePath = filePath;
        Content = content;
        FileContentTable = new();
    }

    public FileDialogResult(FileDialogMode mode, Dictionary<string, string?> fileContentTable)
    {
        Mode = mode;
        FilePath = string.Empty;
        Content = string.Empty;
        FileContentTable = fileContentTable;
    }

    public FileDialogMode Mode { get; }
    public string FilePath { get; set; }
    public string Content { get; set; }
    public Dictionary<string, string?> FileContentTable { get; set; }
}
