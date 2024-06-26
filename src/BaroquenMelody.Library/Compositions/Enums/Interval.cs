namespace BaroquenMelody.Library.Compositions.Enums;

/// <summary>
///    Represents the distance between two notes, normalized to a single octave.
/// </summary>
internal enum Interval : byte
{
    /// <summary>
    ///     A note that is the same as the other note.
    /// </summary>
    Unison = 0,

    /// <summary>
    ///     A note that is one half step away from the other note.
    /// </summary>
    MinorSecond = 1,

    /// <summary>
    ///     A note that is one whole step away from the other note.
    /// </summary>
    MajorSecond = 2,

    /// <summary>
    ///     A note that is one and a half steps away from the other note.
    /// </summary>
    MinorThird = 3,

    /// <summary>
    ///     A note that is two whole steps away from the other note.
    /// </summary>
    MajorThird = 4,

    /// <summary>
    ///     A note that is two and a half steps away from the other note.
    /// </summary>
    PerfectFourth = 5,

    /// <summary>
    ///     A note that is three whole steps away from the other note.
    /// </summary>
    Tritone = 6,

    /// <summary>
    ///     A note that is three and a half steps away from the other note.
    /// </summary>
    PerfectFifth = 7,

    /// <summary>
    ///     A note that is four whole steps away from the other note.
    /// </summary>
    MinorSixth = 8,

    /// <summary>
    ///     A note that is four and a half steps away from the other note.
    /// </summary>
    MajorSixth = 9,

    /// <summary>
    ///     A note that is five whole steps away from the other note.
    /// </summary>
    MinorSeventh = 10,

    /// <summary>
    ///     A note that is five and a half steps away from the other note.
    /// </summary>
    MajorSeventh = 11
}
