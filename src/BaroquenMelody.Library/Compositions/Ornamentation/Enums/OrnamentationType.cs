namespace BaroquenMelody.Library.Compositions.Ornamentation.Enums;

internal enum OrnamentationType : byte
{
    /// <summary>
    ///     No ornamentation.
    /// </summary>
    None = 0,

    /// <summary>
    ///     A passing tone between two notes.
    /// </summary>
    PassingTone,

    /// <summary>
    ///     A sixteenth note run between two notes.
    /// </summary>
    SixteenthNoteRun,

    /// <summary>
    ///     A delayed passing tone between two notes.
    /// </summary>
    DelayedPassingTone,

    /// <summary>
    ///     A turn between two notes.
    /// </summary>
    Turn,

    /// <summary>
    ///     A turn between two notes.
    /// </summary>
    AlternateTurn,

    /// <summary>
    ///     A sustained note over multiple beats.
    /// </summary>
    Sustain,

    /// <summary>
    ///     A rest over multiple beats.
    /// </summary>
    Rest,

    /// <summary>
    ///     A delayed thirty-second note run between two notes.
    /// </summary>
    DelayedThirtySecondNoteRun,

    /// <summary>
    ///     A double turn between two notes.
    /// </summary>
    DoubleTurn,

    /// <summary>
    ///     Two passing tones between two notes.
    /// </summary>
    DoublePassingTone,

    /// <summary>
    ///     A delayed double passing tone between two notes.
    /// </summary>
    DelayedDoublePassingTone,

    /// <summary>
    ///     Outline the dominant seventh chord.
    /// </summary>
    DecorateInterval,

    /// <summary>
    ///    Two thirty-second note runs between two notes.
    /// </summary>
    ThirtySecondNoteRun,

    /// <summary>
    ///     A pedal below a passing tone.
    /// </summary>
    Pedal
}
