namespace BaroquenMelody.Library.Composition.Enums;

/// <summary>
///     Indicates whether a note is arrived at by moving one step or more than one step from the previous note.
/// </summary>
internal enum NoteSpan : byte
{
    /// <summary>
    ///    Indicates that the note is arrived at by moving one step from the previous note.
    /// </summary>
    Step,

    /// <summary>
    ///   Indicates that the note is arrived at by moving more than one step from the previous note.
    /// </summary>
    Leap
}
