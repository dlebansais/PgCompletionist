using System.Windows;

namespace WpfLayout;

public class FileAddedEventArgs : RoutedEventArgs
{
    public FileAddedEventArgs(string filePath)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }
}