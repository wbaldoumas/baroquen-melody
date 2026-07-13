using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;

namespace BaroquenMelody.Library.Scoring.Harmonic;

/// <inheritdoc cref="IScoringRule"/>
/// <remarks>
///     Nudges the harmony toward short Fortspinnung-style sequences: once the two preceding chords establish a
///     root motion (say, a descending fifth), a candidate continuing that motion costs nothing while breaking it
///     costs one — until the motion has already repeated twice, at which point the preference flips so sequences
///     release instead of running away. Chords without an identifiable number are neutral.
/// </remarks>
internal sealed class PreferHarmonicSequences(IChordNumberIdentifier chordNumberIdentifier) : IScoringRule
{
    private const int DegreesInDiatonicScale = 7;

    public double Score(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count < 2)
        {
            return 0d;
        }

        if (GetRootDegree(precedingChords[^2]) is not { } secondLastDegree ||
            GetRootDegree(precedingChords[^1]) is not { } lastDegree ||
            GetRootDegree(nextChord) is not { } nextDegree)
        {
            return 0d;
        }

        var establishedMotion = GetRootMotion(secondLastDegree, lastDegree);

        if (establishedMotion == 0)
        {
            return 0d;
        }

        var candidateContinuesSequence = GetRootMotion(lastDegree, nextDegree) == establishedMotion;

        var sequenceIsSaturated = precedingChords.Count >= 3
            && GetRootDegree(precedingChords[^3]) is { } thirdLastDegree
            && GetRootMotion(thirdLastDegree, secondLastDegree) == establishedMotion;

        if (sequenceIsSaturated)
        {
            return candidateContinuesSequence ? 1d : 0d;
        }

        return candidateContinuesSequence ? 0d : 1d;
    }

    private static int GetRootMotion(int fromDegree, int toDegree) => (toDegree - fromDegree + DegreesInDiatonicScale) % DegreesInDiatonicScale;

    private int? GetRootDegree(BaroquenChord chord)
    {
        var chordNumber = chordNumberIdentifier.IdentifyChordNumber(chord);

        return chordNumber == ChordNumber.Unknown ? null : (int)chordNumber;
    }
}
