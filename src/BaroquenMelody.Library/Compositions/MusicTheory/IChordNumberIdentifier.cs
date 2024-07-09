using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;

namespace BaroquenMelody.Library.Compositions.MusicTheory;

/// <summary>
///     Identifies chord numbers for <see cref="BaroquenNote"/> within the context of a given musical key.
/// </summary>
internal interface IChordNumberIdentifier
{
    /// <summary>
    ///     Identify the chord number of a given <see cref="BaroquenChord"/>.
    /// </summary>
    /// <param name="chord">The <see cref="BaroquenChord"/> to identify the chord number of.</param>
    /// <returns>The chord number of the given <see cref="BaroquenChord"/>.</returns>
    ChordNumber IdentifyChordNumber(BaroquenChord chord);
}
