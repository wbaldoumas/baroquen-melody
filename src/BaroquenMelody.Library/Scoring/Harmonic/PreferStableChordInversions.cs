using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;

namespace BaroquenMelody.Library.Scoring.Harmonic;

/// <inheritdoc cref="IScoringRule"/>
/// <remarks>
///     Second-inversion (six-four) sonorities are reserved for a few specific contexts in the Baroque idiom,
///     so a candidate voicing the fifth of its chord in the bass costs one. Root position and first inversion
///     are freely usable and cost nothing, as do chords whose inversion cannot be identified.
/// </remarks>
internal sealed class PreferStableChordInversions(IChordInversionIdentifier chordInversionIdentifier) : IScoringRule
{
    public double Score(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord) =>
        chordInversionIdentifier.IdentifyChordInversion(nextChord) == ChordInversion.SecondInversion ? 1d : 0d;
}
