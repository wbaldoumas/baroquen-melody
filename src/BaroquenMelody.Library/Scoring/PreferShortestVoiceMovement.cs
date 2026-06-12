using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Scoring;

/// <inheritdoc cref="IScoringRule"/>
/// <remarks>
///     Rameau's "law of the shortest way": voices should retain common tones and otherwise move as little as
///     possible. The penalty is the total scale-step distance moved across all voices, so a held note costs
///     nothing, a step costs one, and leaps cost their full size.
/// </remarks>
internal sealed class PreferShortestVoiceMovement(CompositionConfiguration compositionConfiguration) : IScoringRule
{
    public double Score(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count == 0)
        {
            return 0d;
        }

        var lastChord = precedingChords[^1];
        var penalty = 0d;

        foreach (var nextNote in nextChord.Notes)
        {
            if (!lastChord.ContainsInstrument(nextNote.Instrument))
            {
                continue;
            }

            var lastNoteScaleIndex = compositionConfiguration.Scale.IndexOf(lastChord[nextNote.Instrument]);
            var nextNoteScaleIndex = compositionConfiguration.Scale.IndexOf(nextNote);

            if (lastNoteScaleIndex < 0 || nextNoteScaleIndex < 0)
            {
                continue;
            }

            penalty += Math.Abs(nextNoteScaleIndex - lastNoteScaleIndex);
        }

        return penalty;
    }
}
