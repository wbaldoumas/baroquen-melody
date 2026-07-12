using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Rules.Harmonic;

/// <inheritdoc cref="ICompositionRule"/>
/// <remarks>
///     Voices are ordered from highest register to lowest via <see cref="CompositionConfiguration.Instruments"/>. A
///     crossing occurs when a lower-register voice sounds above the adjacent higher-register voice. Equal pitches
///     (a unison) are not a crossing.
/// </remarks>
internal sealed class AvoidVoiceCrossing(CompositionConfiguration compositionConfiguration) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        var voices = compositionConfiguration.Instruments.Where(nextChord.ContainsInstrument).ToList();

        for (var lowerVoiceIndex = 1; lowerVoiceIndex < voices.Count; ++lowerVoiceIndex)
        {
            var higherRegisterNote = nextChord[voices[lowerVoiceIndex - 1]];
            var lowerRegisterNote = nextChord[voices[lowerVoiceIndex]];

            if (lowerRegisterNote > higherRegisterNote)
            {
                return false;
            }
        }

        return true;
    }
}
