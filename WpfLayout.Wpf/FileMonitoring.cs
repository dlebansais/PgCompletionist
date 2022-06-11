namespace WpfLayout;

using System.IO;

public static class FileMonitoring
{
    public static void Install(string folderPath, string filter)
    {
        Uninstall();

        try
        {
            Watcher = new FileSystemWatcher();
            Watcher.Path = folderPath;

            Watcher.NotifyFilter = NotifyFilters.LastWrite;
            Watcher.Filter = filter;
            Watcher.Changed += OnFileAdded;
            Watcher.EnableRaisingEvents = true;
        }
        catch
        {
            Watcher = null;
        }
    }

    public static void Uninstall()
    {
        if (Watcher is not null)
        {
            using (Watcher)
            {
                Watcher.EnableRaisingEvents = false;
                Watcher.Changed -= OnFileAdded;
            }

            Watcher = null!;
        }
    }

    public static event FileAddedEventHandler? FileAdded;

    private static void NotifyFileAdded(string filePath)
    {
        FileAdded?.Invoke(null!, new FileAddedEventArgs(filePath));
    }

    private static void OnFileAdded(object sender, FileSystemEventArgs args)
    {
        string FilePath = args.FullPath;
        NotifyFileAdded(FilePath);
    }

    private static FileSystemWatcher? Watcher = null;
}
