namespace WpfLayout;

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TaskbarTools;

public static class IconTools
{
    #region Init
    static IconTools()
    {
        if (FindIconFolder(out string Folder))
            IconFolder = Folder;
    }

    public static void Initialize(Window window, string expectedVersion, int iconId, string applicationName)
    {
        if (Icon != ImageConversion.DefaultImage)
            window.Icon = Icon;

        TaskDispatcher = TaskDispatcher.Create(window);
        ExpectedVersion = expectedVersion;

        if (IconFolder.Length > 0)
            UpdateTaskbarLink(iconId, applicationName);
    }

    private static void UpdateTaskbarLink(int iconId, string applicationName)
    {
        string ImageFile = Path.Combine(IconFolder, $"icon_{iconId}.png");
        if (ImageFileToIconFile(ImageFile, "mainicon.p", "mainicon.i", out string IconFile))
        {
            Icon = ImageConversion.IconFileToImageSource(IconFile);
            TaskbarShortcut.UpdateTaskbarShortcut($"{applicationName}.lnk", IconFile, out _);
        }
    }

    private static TaskDispatcher TaskDispatcher = TaskDispatcher.Discard;
    private static string ExpectedVersion = string.Empty;

    private static bool FindIconFolder(out string folder)
    {
        folder = string.Empty;

        string UserRootFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string ApplicationFolder = Path.Combine(UserRootFolder, "PgJsonParse");
        string VersionCacheFolder = Path.Combine(ApplicationFolder, "Versions");

        if (!Directory.Exists(VersionCacheFolder))
            return false;

        string? FinalFolder = null;
        string SharedFolder = Path.Combine(ApplicationFolder, "Shared Icons");

        if (Directory.Exists(SharedFolder))
            FinalFolder = SharedFolder;
        else
        {
            string[] VersionFolders = Directory.GetDirectories(VersionCacheFolder);
            int LastVersion = -1;

            foreach (string Folder in VersionFolders)
            {
                if (int.TryParse(Path.GetFileName(Folder), out int FolderVersion))
                    if (LastVersion < FolderVersion)
                        LastVersion = FolderVersion;
            }

            if (LastVersion > 0)
                FinalFolder = Path.Combine(VersionCacheFolder, LastVersion.ToString(CultureInfo.InvariantCulture));
        }

        if (FinalFolder != null)
        {
            folder = FinalFolder;
            return true;
        }
        else
            return false;
    }

    private static bool ImageFileToIconFile(string imageFile, string resourcePngName, string resourceIcoName, out string iconFile)
    {
        iconFile = string.Empty;

        if (!File.Exists(imageFile))
            return false;

        string IconFileAsIco = Path.ChangeExtension(imageFile, "ico");
        if (!File.Exists(IconFileAsIco))
        {
            using System.Drawing.Bitmap FileBitmap = new System.Drawing.Bitmap(imageFile, true);
            using System.Drawing.Bitmap ResourceBitmap = new System.Drawing.Bitmap(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/" + resourcePngName)).Stream);
            if (AreBitmapsEqual(FileBitmap, ResourceBitmap))
            {
                using System.Drawing.Icon ResourceIcon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/" + resourceIcoName)).Stream);
                using (FileStream fs = new FileStream(IconFileAsIco, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    ResourceIcon.Save(fs);
                }
            }
        }

        if (!File.Exists(IconFileAsIco))
            return false;

        iconFile = IconFileAsIco;
        return true;
    }

    public static bool AreBitmapsEqual(System.Drawing.Bitmap b1, System.Drawing.Bitmap b2)
    {
        if (b1 == null || b2 == null)
            return false;

        if (b1.Size != b2.Size)
            return false;

        BitmapData bd1 = b1.LockBits(new System.Drawing.Rectangle(new System.Drawing.Point(0, 0), b1.Size), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        BitmapData bd2 = b2.LockBits(new System.Drawing.Rectangle(new System.Drawing.Point(0, 0), b2.Size), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        try
        {
            IntPtr bd1scan0 = bd1.Scan0;
            IntPtr bd2scan0 = bd2.Scan0;

            int stride = bd1.Stride;
            int len = stride * b1.Height;

            byte[] bd1scan0bytes = new byte[len];
            Marshal.Copy(bd1scan0, bd1scan0bytes, 0, len);
            byte[] bd2scan0bytes = new byte[len];
            Marshal.Copy(bd2scan0, bd2scan0bytes, 0, len);

            for (int i = 0; i < len; i++)
                if (bd1scan0bytes[i] != bd2scan0bytes[i])
                    return false;

            return true;
        }
        finally
        {
            b1.UnlockBits(bd1);
            b2.UnlockBits(bd2);
        }
    }
    #endregion

    #region Properties
    private static string IconFolder { get; } = string.Empty;
    public static ImageSource Icon { get; private set; } = ImageConversion.DefaultImage;
    #endregion

    #region Client Interface
    public static string IdToFile(int iconId)
    {
        if (IconFolder == null)
            return string.Empty;

        string FileName = $"icon_{iconId}.png";
        string IconFile = Path.Combine(IconFolder, FileName);

        if (!File.Exists(IconFile))
            StartDownloadThread(IconFile);

        return IconFile;
    }

    public static Image IdToImage(int iconId)
    {
        Image NewImage = new Image();
        NewImage.Source = ImageConversion.IconFileToImageSource(IdToFile(iconId));

        return NewImage;
    }

    public static void Close()
    {
        DownloadThread = NullThread;
    }
    #endregion

    #region Download
    private static void StartDownloadThread(string iconFile)
    {
        FileQueue.Enqueue(iconFile);

        if (DownloadThread != NullThread)
            return;

        DownloadThread = new Thread(new ThreadStart(ExecuteDownload));
        DownloadThread.Start();
    }

    private static void ExecuteDownload()
    {
        bool RefreshDisplay = false;

        while (DownloadThread != NullThread)
        {
            if (FileQueue.Count > 0)
            {
                string IconFile = FileQueue.Dequeue();
                ExecuteDownload(IconFile);

                RefreshDisplay = true;
            }
            else
            {
                if (RefreshDisplay)
                {
                    RefreshDisplay = false;
                    TaskDispatcher.Dispatch(new Action(OnRefreshDisplay));
                }

                Thread.Sleep(500);
            }
        }
    }

    private static void OnRefreshDisplay()
    {
        Window Window = Application.Current.MainWindow;
        OnRefreshDisplay(Window);
    }

    private static void OnRefreshDisplay(DependencyObject obj)
    {
        if (obj is Image AsImage)
        {
            BindingExpression SourcePropertyBinding = BindingOperations.GetBindingExpression(AsImage, Image.SourceProperty);
            if (SourcePropertyBinding != null)
                SourcePropertyBinding.UpdateTarget();
        }

        int ChildrenCount = VisualTreeHelper.GetChildrenCount(obj);

        for (int i = 0; i < ChildrenCount; i++)
            OnRefreshDisplay(VisualTreeHelper.GetChild(obj, i));
    }

    private static void ExecuteDownload(string iconFile)
    {
        string Version = ExpectedVersion;
        string SourceLocation = "icons";
        string IconName = Path.GetFileName(iconFile);
        string RequestUri = $"http://cdn.projectgorgon.com/{Version}/{SourceLocation}/{IconName}";
        try
        {
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(new Uri(RequestUri));
            using WebResponse Response = Request.GetResponse();
            using Stream ResponseStream = Response.GetResponseStream();
            using BinaryReader Reader = new BinaryReader(ResponseStream);
            byte[] Content = Reader.ReadBytes((int)Response.ContentLength);

            using FileStream Stream = new FileStream(iconFile, FileMode.Create, FileAccess.Write);
            using BinaryWriter Writer = new BinaryWriter(Stream);
            Writer.Write(Content);
        }
        catch (FormatException)
        {
            DownloadThread = NullThread;
        }
        catch (InvalidOperationException)
        {
            DownloadThread = NullThread;
        }
        catch (SecurityException)
        {
            DownloadThread = NullThread;
        }
        catch (NotSupportedException)
        {
            DownloadThread = NullThread;
        }
    }

    private static Thread NullThread = new Thread(new ThreadStart(() => { }));
    private static Thread DownloadThread = NullThread;
    private static Queue<string> FileQueue = new Queue<string>();
    #endregion
}
