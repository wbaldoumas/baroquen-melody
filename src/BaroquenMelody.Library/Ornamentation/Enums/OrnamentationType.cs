namespace BaroquenMelody.Library.Ornamentation.Enums;

public enum OrnamentationType : byte
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
    ///     A run between two notes.
    /// </summary>
    Run,

    /// <summary>
    ///     A delayed passing tone between two notes.
    /// </summary>
    DelayedPassingTone,

    /// <summary>
    ///     A turn between two notes.
    /// </summary>
    Turn,

    /// <summary>
    ///     An inverted turn between two notes.
    /// </summary>
    InvertedTurn,

    /// <summary>
    ///     A sustained note over multiple beats.
    /// </summary>
    Sustain,

    /// <summary>
    ///     A note that is part of another sustained note.
    /// </summary>
    MidSustain,

    /// <summary>
    ///     A delayed run between two notes.
    /// </summary>
    DelayedRun,

    /// <summary>
    ///     A double turn between two notes.
    /// </summary>
    DoubleTurn,

    /// <summary>
    ///     A double turn between two notes.
    /// </summary>
    DoubleInvertedTurn,

    /// <summary>
    ///     Two passing tones between two notes.
    /// </summary>
    DoublePassingTone,

    /// <summary>
    ///     A delayed double passing tone between two notes.
    /// </summary>
    DelayedDoublePassingTone,

    /// <summary>
    ///     Outlines an interval in a chord.
    /// </summary>
    DecorateInterval,

    /// <summary>
    ///    Two back to back runs between two notes.
    /// </summary>
    DoubleRun,

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
    ///     A note which is repeated across two notes.
    /// </summary>
    RepeatedNote,

    /// <summary>
    ///     A delayed note which is repeated across two notes.
    /// </summary>
    DelayedRepeatedNote,

    /// <summary>
    ///     A neighbor tone to ornament an existing note.
    /// </summary>
    NeighborTone,

    /// <summary>
    ///     A delayed neighbor tone to ornament an existing note.
    /// </summary>
    DelayedNeighborTone,

    /// <summary>
    ///     A pickup note before another note.
    /// </summary>
    Pickup,

    /// <summary>
    ///     A delayed pickup note before another note.
    /// </summary>
    DelayedPickup,

    /// <summary>
    ///     A double pickup note before another note.
    /// </summary>
    DoublePickup,

    /// <summary>
    ///     A delayed double pickup note before another note.
    /// </summary>
    DelayedDoublePickup,

    /// <summary>
    ///     A decoration to a lower third from the starting note.
    /// </summary>
    DecorateThird,

    /// <summary>
    ///     A pedal one octave below the starting note.
    /// </summary>
    OctavePedal,

    /// <summary>
    ///     An octave pedal with a passing tone.
    /// </summary>
    OctavePedalPassingTone,

    /// <summary>
    ///     An octave pedal with an arpeggio.
    /// </summary>
    OctavePedalArpeggio,
}
