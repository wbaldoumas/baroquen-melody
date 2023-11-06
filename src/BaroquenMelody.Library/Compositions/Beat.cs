namespace BaroquenMelody.Library.Compositions;

/// <summary>
///     Represents a beat in a composition.
/// </summary>
/// <param name="chord"> The chord that is played during the beat. </param>
internal sealed record Beat(
    Chord chord
);
