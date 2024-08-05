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
    ///     A note that is part of another sustained note.
    /// </summary>
    MidSustain,

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
    Pedal,

    /// <summary>
    ///     A mordent accentuating a note.
    /// </summary>
    Mordent,

    /// <summary>
    ///     A rest.
    /// </summary>
    Rest,

    /// <summary>
    ///     A note which is repeated across two eighth notes.
    /// </summary>
    RepeatedEighthNote,

    /// <summary>
    ///     A note which is repeated across one dotted eighth note followed by a sixteenth note.
    /// </summary>
    RepeatedDottedEighthSixteenth,

    /// <summary>
    ///     A neighbor tone to ornament an existing note.
    /// </summary>
    NeighborTone,

    /// <summary>
    ///     A delayed neighbor tone to ornament an existing note.
    /// </summary>
    DelayedNeighborTone
}
