namespace BaroquenMelody.App.Infrastructure.FileSystem;

/// <summary>
///     Shows a native Windows "Save As" dialog. Implemented with the classic Win32 common
///     dialog (<c>comdlg32.dll</c>) because the WinRT <c>FileSavePicker</c> throws
///     <c>E_FAIL (0x80004005)</c> in unpackaged and elevated desktop processes.
/// </summary>
internal interface IWindowsSaveFileDialog
{
    /// <summary>
    ///     Prompts the user for a destination path.
    /// </summary>
    /// <param name="suggestedFileName">The file name to pre-populate the dialog with.</param>
    /// <returns>The chosen full path, or <see langword="null"/> if the user cancelled.</returns>
    string? Show(string suggestedFileName);
}
