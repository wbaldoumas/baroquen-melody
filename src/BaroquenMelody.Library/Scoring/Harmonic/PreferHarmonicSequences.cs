using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Viewpoints;

namespace BaroquenMelody.Library.Scoring.Harmonic;

/// <inheritdoc cref="IScoringRule"/>
/// <remarks>
///     Nudges the harmony toward short Fortspinnung-style sequences: once the two preceding harmonies establish a
///     root motion (say, a descending fifth), a candidate continuing that motion costs nothing while breaking it
///     costs one — until the motion has already repeated twice, at which point the preference flips so sequences
///     release instead of running away. The context is read as distinct harmonic events, so a harmony held across
///     duplicated beats is one sequence member rather than a zero motion that erases the pattern. The release
///     needs three distinct events in view, and holds halve a window's event density: a fully held window at the
///     default context size carries only two, so there the rule is a one-way continuation nudge and the release
///     engages only where fresh beats or a larger context bring a third event into the window. Chords without an
///     identifiable number are neutral.
/// </remarks>
internal sealed class PreferHarmonicSequences(IChordNumberIdentifier chordNumberIdentifier) : IScoringRule
{
    private const int DegreesInDiatonicScale = 7;

    public double Score(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (HarmonicEvents.PrecedingEventChord(precedingChords, 1) is not { } lastEventChord ||
            HarmonicEvents.PrecedingEventChord(precedingChords, 2) is not { } secondLastEventChord)
        {
            return 0d;
        }

        if (GetRootDegree(secondLastEventChord) is not { } secondLastDegree ||
            GetRootDegree(lastEventChord) is not { } lastDegree ||
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

        var sequenceIsSaturated = HarmonicEvents.PrecedingEventChord(precedingChords, 3) is { } thirdLastEventChord
            && GetRootDegree(thirdLastEventChord) is { } thirdLastDegree
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
