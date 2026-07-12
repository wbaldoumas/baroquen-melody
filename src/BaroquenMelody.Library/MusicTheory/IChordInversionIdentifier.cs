using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory.Enums;

namespace BaroquenMelody.Library.MusicTheory;

/// <summary>
///     Identifies chord inversions for <see cref="BaroquenChord"/> within the context of a given musical key.
/// </summary>
internal interface IChordInversionIdentifier
{
    /// <summary>
    ///     Identify the inversion of a given <see cref="BaroquenChord"/> from its bass voice.
    /// </summary>
    /// <param name="chord">The <see cref="BaroquenChord"/> to identify the inversion of.</param>
    /// <returns>The inversion of the given <see cref="BaroquenChord"/>.</returns>
    ChordInversion IdentifyChordInversion(BaroquenChord chord);
}
