using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Viewpoints;

namespace BaroquenMelody.Library.Scoring.Melodic;

/// <inheritdoc cref="IMelodicScoringRule"/>
/// <remarks>
///     After a melodic leap (three or more scale steps), a voice should recover with a one- or two-step move in
///     the opposite direction. A leap left unrecovered costs one.
/// </remarks>
internal sealed class PreferLeapRecovery(CompositionConfiguration compositionConfiguration) : IMelodicScoringRule
{
    private const int LeapThresholdInScaleSteps = 3;

    private const int MaxRecoveryDistanceInScaleSteps = 2;

    public double Score(in MelodicLine line)
    {
        var secondLastNote = line.PrecedingNote(2);
        var lastNote = line.PrecedingNote(1);

        if (secondLastNote is null || lastNote is null)
        {
            return 0d;
        }

        var secondLastNoteScaleIndex = compositionConfiguration.Scale.IndexOf(secondLastNote);
        var lastNoteScaleIndex = compositionConfiguration.Scale.IndexOf(lastNote);
        var nextNoteScaleIndex = compositionConfiguration.Scale.IndexOf(line.NextNote);

        if (secondLastNoteScaleIndex < 0 || lastNoteScaleIndex < 0 || nextNoteScaleIndex < 0)
        {
            return 0d;
        }

        var previousMove = lastNoteScaleIndex - secondLastNoteScaleIndex;

        if (Math.Abs(previousMove) < LeapThresholdInScaleSteps)
        {
            return 0d;
        }

        var nextMove = nextNoteScaleIndex - lastNoteScaleIndex;

        // The leap is "recovered" when this next move steps back the other way. Both parts must hold: it heads the
        // opposite direction to the leap (the signs are opposite, so a held or repeated note - sign 0 - does not
        // count), and it is small (at most MaxRecoveryDistanceInScaleSteps), i.e. a step or two rather than another leap.
        var isRecovered = Math.Sign(nextMove) == -Math.Sign(previousMove) && Math.Abs(nextMove) <= MaxRecoveryDistanceInScaleSteps;

        return isRecovered ? 0d : 1d;
    }
}
