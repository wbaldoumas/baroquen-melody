using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Scoring;

/// <inheritdoc cref="IScoringRule"/>
/// <remarks>
///     After a melodic leap (three or more scale steps), a voice should recover with a one- or two-step move in
///     the opposite direction. Each voice whose leap is left unrecovered costs one.
/// </remarks>
internal sealed class PreferLeapRecovery(CompositionConfiguration compositionConfiguration) : IScoringRule
{
    private const int LeapThresholdInScaleSteps = 3;

    private const int MaxRecoveryDistanceInScaleSteps = 2;

    public double Score(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count < 2)
        {
            return 0d;
        }

        var secondLastChord = precedingChords[^2];
        var lastChord = precedingChords[^1];
        var penalty = 0d;

        foreach (var nextNote in nextChord.Notes)
        {
            if (!secondLastChord.ContainsInstrument(nextNote.Instrument) || !lastChord.ContainsInstrument(nextNote.Instrument))
            {
                continue;
            }

            var secondLastNoteScaleIndex = compositionConfiguration.Scale.IndexOf(secondLastChord[nextNote.Instrument]);
            var lastNoteScaleIndex = compositionConfiguration.Scale.IndexOf(lastChord[nextNote.Instrument]);
            var nextNoteScaleIndex = compositionConfiguration.Scale.IndexOf(nextNote);

            if (secondLastNoteScaleIndex < 0 || lastNoteScaleIndex < 0 || nextNoteScaleIndex < 0)
            {
                continue;
            }

            var previousMove = lastNoteScaleIndex - secondLastNoteScaleIndex;

            if (Math.Abs(previousMove) < LeapThresholdInScaleSteps)
            {
                continue;
            }

            var nextMove = nextNoteScaleIndex - lastNoteScaleIndex;
            var isRecovered = Math.Sign(nextMove) == -Math.Sign(previousMove) && Math.Abs(nextMove) <= MaxRecoveryDistanceInScaleSteps;

            if (!isRecovered)
            {
                ++penalty;
            }
        }

        return penalty;
    }
}
