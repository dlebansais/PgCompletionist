namespace WpfLayout;

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

public static class SystemTools
{
    #region Init
    static SystemTools()
    {
        IsRunningInBrowser = false;
        IsInitialized = true;
        IsExecutingOnWindows = true;
        AppVersion = string.Empty;
    }
    #endregion

    #region Properties
    public static bool IsRunningInBrowser { get; }
    public static bool IsInitialized { get; }
    public static bool IsExecutingOnWindows { get; }
    public static string AppVersion { get; }
    public static bool IsAppVersionKnown { get { return AppVersion.Length > 0; } }
    public static bool IsGlobalizationInvariantMode { get { return new string(new char[] { '\u0063', '\u0301', '\u0327', '\u00BE' }).IsNormalized(); } }
    #endregion

    #region Client Interface
    public static string? GetVersion()
    {
        Assembly CurrentAssembly = Assembly.GetEntryAssembly();
#pragma warning disable IL3000 // Avoid using accessing Assembly file path when publishing as a single-file
        FileVersionInfo VersionInfo = FileVersionInfo.GetVersionInfo(CurrentAssembly.Location);
#pragma warning restore IL3000 // Avoid using accessing Assembly file path when publishing as a single-file

        return VersionInfo.FileVersion;
    }

    public static async Task<byte[]> GetResourceFile(string name)
    {
        Assembly CurrentAssembly = Assembly.GetEntryAssembly();
        string[] ManifestResourceNames = CurrentAssembly.GetManifestResourceNames();

        foreach (string ResourceName in ManifestResourceNames)
        {
            if (ResourceName.EndsWith($".Resources.{name}"))
            {
                using Stream? ResourceStream = CurrentAssembly.GetManifestResourceStream(ResourceName);
                if (ResourceStream is not null)
                {
                    using BinaryReader Reader = new BinaryReader(ResourceStream);
                    await Task.CompletedTask;
                    return Reader.ReadBytes((int)ResourceStream.Length);
                }
            }
        }

        throw new InvalidDataException();
    }
    #endregion
}
