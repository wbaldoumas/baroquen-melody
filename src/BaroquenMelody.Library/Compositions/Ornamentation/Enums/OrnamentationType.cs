namespace BaroquenMelody.Library.Compositions.Ornamentation.Enums;

internal enum OrnamentationType : byte
{
    /// <summary>
    ///     A passing tone between two notes.
    /// </summary>
    PassingTone,

    /// <summary>
    ///     A sixteenth note run between two notes.
    /// </summary>
    SixteenthNoteRun,

    /// <summary>
    ///    A delayed passing tone between two notes.
    /// </summary>
    DelayedPassingTone,

    /// <summary>
    ///    A turn between two notes.
    /// </summary>
    Turn
}
