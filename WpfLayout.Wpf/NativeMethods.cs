namespace WpfLayout;

using System;
using System.Runtime.InteropServices;

public static class NativeMethods
{
    #region P/Invoke imports & definitions
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32", EntryPoint = "SetWindowPos")]
    public static extern void SetWindowPos(IntPtr hWnd, int hwndInsertAfter, int x, int y, int cx, int cy, int wFlags);

    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteObject([In] IntPtr hObject);
    #endregion

    #region System Folders
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
    #endregion
}
