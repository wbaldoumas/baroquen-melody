using System.Runtime.InteropServices;

namespace BaroquenMelody.App.Infrastructure.FileSystem;

/// <inheritdoc cref="IWindowsSaveFileDialog" />
internal sealed class WindowsSaveFileDialog : IWindowsSaveFileDialog
{
    private const int MaxPathLength = 260;
    private const int OfnOverwritePrompt = 0x00000002;
    private const int OfnPathMustExist = 0x00000800;
    private const int OfnNoChangeDir = 0x00000008;
    private const int OfnExplorer = 0x00080000;

    public string? Show(string suggestedFileName)
    {
        var owner = ResolveActiveWindowHandle();

        // The filter contains embedded nulls, so it must be allocated manually rather than
        // marshalled as a normal string (which would truncate at the first null).
        var filter = "MIDI files (*.mid)\0*.mid\0All files (*.*)\0*.*\0\0";
        var filterPtr = Marshal.StringToHGlobalUni(filter);
        var filePtr = Marshal.AllocHGlobal(MaxPathLength * sizeof(char));

        try
        {
            var initialChars = new char[MaxPathLength];
            var length = Math.Min(suggestedFileName.Length, MaxPathLength - 1);
            suggestedFileName.CopyTo(0, initialChars, 0, length);
            Marshal.Copy(initialChars, 0, filePtr, MaxPathLength);

            var ofn = new OpenFileName
            {
                lStructSize = Marshal.SizeOf<OpenFileName>(),
                hwndOwner = owner,
                lpstrFilter = filterPtr,
                nFilterIndex = 1,
                lpstrFile = filePtr,
                nMaxFile = MaxPathLength,
                lpstrDefExt = "mid",
                Flags = OfnOverwritePrompt | OfnPathMustExist | OfnNoChangeDir | OfnExplorer
            };

            return GetSaveFileName(ref ofn) ? Marshal.PtrToStringUni(filePtr) : null;
        }
        finally
        {
            Marshal.FreeHGlobal(filePtr);
            Marshal.FreeHGlobal(filterPtr);
        }
    }

    private static IntPtr ResolveActiveWindowHandle()
    {
        var windows = Microsoft.Maui.Controls.Application.Current?.Windows;
        var window = windows is { Count: > 0 } ? windows[0].Handler?.PlatformView as Microsoft.UI.Xaml.Window : null;

        return window is null ? IntPtr.Zero : WinRT.Interop.WindowNative.GetWindowHandle(window);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
#pragma warning disable SA1307 // Field names mirror the native OPENFILENAMEW structure.
    private struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public IntPtr lpstrFilter;
        public IntPtr lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public IntPtr lpstrFile;
        public int nMaxFile;
        public IntPtr lpstrFileTitle;
        public int nMaxFileTitle;
        public IntPtr lpstrInitialDir;
        public IntPtr lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public IntPtr lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }
#pragma warning restore SA1307

    [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSaveFileName(ref OpenFileName ofn);
}
