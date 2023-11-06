namespace BaroquenMelody.Library.Compositions.Enums;

/// <summary>
///   Indicates the motion taken to arrive at a given note from the previous note.
/// </summary>
internal enum NoteMotion : byte
{
    /// <summary>
    ///     Indicates that the note is arrived at by moving up from the previous note.
    /// </summary>
    Ascending,

    /// <summary>
    ///     Indicates that the note is arrived at by moving down from the previous note.
    /// </summary>
    Descending,

    /// <summary>
    ///     Indicates that the note is arrived at by staying at the same pitch as the previous note.
    /// </summary>
    Oblique
}
