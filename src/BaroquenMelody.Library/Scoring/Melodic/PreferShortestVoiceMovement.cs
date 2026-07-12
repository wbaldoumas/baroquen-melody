using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Viewpoints;

namespace BaroquenMelody.Library.Scoring.Melodic;

/// <inheritdoc cref="IMelodicScoringRule"/>
/// <remarks>
///     Rameau's "law of the shortest way": a voice should retain common tones and otherwise move as little as
///     possible. The penalty is the scale-step distance the voice moves, so a held note costs nothing, a step
///     costs one, and leaps cost their full size.
/// </remarks>
internal sealed class PreferShortestVoiceMovement(CompositionConfiguration compositionConfiguration) : IMelodicScoringRule
{
    public double Score(in MelodicLine line)
    {
        var lastNote = line.PrecedingNote(1);

        if (lastNote is null)
        {
            return 0d;
        }

        var lastNoteScaleIndex = compositionConfiguration.Scale.IndexOf(lastNote);
        var nextNoteScaleIndex = compositionConfiguration.Scale.IndexOf(line.NextNote);

        if (lastNoteScaleIndex < 0 || nextNoteScaleIndex < 0)
        {
            return 0d;
        }

        return Math.Abs(nextNoteScaleIndex - lastNoteScaleIndex);
    }
}
