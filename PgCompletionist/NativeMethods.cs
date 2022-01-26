namespace PgCompletionist;

using System;
using System.Runtime.InteropServices;

internal static class NativeMethods
{
    public static readonly Guid LocalLowId = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");

    public static string GetKnownFolderPath(Guid knownFolderId)
    {
        IntPtr pszPath = IntPtr.Zero;
        try
        {
            int hr = SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath);
            if (hr >= 0)
                return Marshal.PtrToStringAuto(pszPath)!;
            throw Marshal.GetExceptionForHR(hr)!;
        }
        finally
        {
            if (pszPath != IntPtr.Zero)
                Marshal.FreeCoTaskMem(pszPath);
        }
    }

    [DllImport("shell32.dll")]
    private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);
}
