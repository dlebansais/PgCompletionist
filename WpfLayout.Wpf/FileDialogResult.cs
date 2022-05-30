namespace WpfLayout;

public record FileDialogResult
{
    public FileDialogResult(FileDialogMode mode, string filePath, string content)
    {
        Mode = mode;
        FilePath = filePath;
        Content = content;
    }

    public FileDialogResult(FileDialogMode mode, string filePath)
    {
        Mode = mode;
        FilePath = filePath;
        Content = string.Empty;
    }

    public FileDialogMode Mode { get; }
    public string FilePath { get; set; }
    public string Content { get; set; }
}
