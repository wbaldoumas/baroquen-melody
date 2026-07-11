using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Rules.Harmonic;

/// <inheritdoc cref="ICompositionRule"/>
/// <remarks>
///     Voices are ordered from highest register to lowest via <see cref="CompositionConfiguration.Instruments"/>. Every
///     adjacent pair of upper voices must span at most one octave; the lowest adjacent pair (bass and tenor) is left
///     unrestricted, as is the case for fewer than three voices.
/// </remarks>
internal sealed class EnforceVoiceSpacing(CompositionConfiguration compositionConfiguration) : ICompositionRule
{
    /// <summary>
    ///     The widest interval, in semitones, allowed between adjacent upper voices: one octave. Shared with
    ///     <see cref="VoiceSpacingSatisfiabilityAnalyzer"/> so satisfiability analysis and enforcement cannot drift apart.
    /// </summary>
    internal const int MaximumAdjacentVoiceSpacing = 12;

    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        var voices = compositionConfiguration.Instruments.Where(nextChord.ContainsInstrument).ToList();

        // skip the lowest adjacent pair (the final index) since bass-tenor spacing is unrestricted.
        for (var higherVoiceIndex = 0; higherVoiceIndex < voices.Count - 2; ++higherVoiceIndex)
        {
            var higherNote = nextChord[voices[higherVoiceIndex]];
            var lowerNote = nextChord[voices[higherVoiceIndex + 1]];

            if (Math.Abs((int)higherNote.NoteNumber - (int)lowerNote.NoteNumber) > MaximumAdjacentVoiceSpacing)
            {
                return false;
            }
        }

        return true;
    }
}
