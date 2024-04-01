using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Domain;

/// <summary>
///     Represents a beat in a composition.
/// </summary>
/// <param name="Chord"> The chord that is played during the beat. </param>
internal sealed record Beat(BaroquenChord Chord)
{
    public BaroquenNote this[Voice voice] => Chord[voice];
}
