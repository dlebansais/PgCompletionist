namespace WpfLayout;

using System;
using System.Collections.Generic;

public record FileDialogResult
{
    public FileDialogResult(FileDialogMode mode, string filePath)
        : this(mode, filePath, Array.Empty<byte>())
    {
    }

    public FileDialogResult(FileDialogMode mode, string filePath, byte[] contentBytes)
    {
        Mode = mode;
        FilePath = filePath;
        ContentBytes = contentBytes;
        FileContentBytesTable = new Dictionary<string, byte[]?>();
    }

    public FileDialogResult(FileDialogMode mode, Dictionary<string, byte[]?> fileContentBytesTable)
    {
        Mode = mode;
        FilePath = string.Empty;
        ContentBytes = Array.Empty<byte>();
        FileContentBytesTable = fileContentBytesTable;
    }

    public FileDialogMode Mode { get; }
    public string FilePath { get; set; }
    public byte[] ContentBytes { get; set; }
    public Dictionary<string, byte[]?> FileContentBytesTable { get; set; }
}
