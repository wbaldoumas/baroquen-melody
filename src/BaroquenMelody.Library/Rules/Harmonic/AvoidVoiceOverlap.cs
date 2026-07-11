using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Rules.Harmonic;

/// <inheritdoc cref="ICompositionRule"/>
/// <remarks>
///     Voices are ordered from highest register to lowest via <see cref="CompositionConfiguration.Instruments"/>. An
///     overlap occurs when a voice moves strictly above the previous position of its adjacent higher voice, or strictly
///     below the previous position of its adjacent lower voice. Moving to exactly the adjacent voice's previous pitch is
///     permitted.
/// </remarks>
internal sealed class AvoidVoiceOverlap(CompositionConfiguration compositionConfiguration) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count == 0)
        {
            return true;
        }

        var previousChord = precedingChords[^1];

        var voices = compositionConfiguration.Instruments
            .Where(instrument => nextChord.ContainsInstrument(instrument) && previousChord.ContainsInstrument(instrument))
            .ToList();

        for (var lowerVoiceIndex = 1; lowerVoiceIndex < voices.Count; ++lowerVoiceIndex)
        {
            var higherVoice = voices[lowerVoiceIndex - 1];
            var lowerVoice = voices[lowerVoiceIndex];

            var lowerVoiceOverlapsAbove = nextChord[lowerVoice] > previousChord[higherVoice];
            var higherVoiceOverlapsBelow = nextChord[higherVoice] < previousChord[lowerVoice];

            if (lowerVoiceOverlapsAbove || higherVoiceOverlapsBelow)
            {
                return false;
            }
        }

        return true;
    }
}
