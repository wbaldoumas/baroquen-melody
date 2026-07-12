using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Enums.Extensions;

namespace BaroquenMelody.Library.Scoring.Harmonic;

/// <inheritdoc cref="IScoringRule"/>
/// <remarks>
///     The outer voices (the highest- and lowest-register instruments per
///     <see cref="CompositionConfiguration.Instruments"/>) should by default move in contrary or oblique motion.
///     Similar motion costs one, except when the bass leaps a perfect fourth or fifth, which licenses it.
/// </remarks>
internal sealed class PreferContraryOuterVoiceMotion(CompositionConfiguration compositionConfiguration) : IScoringRule
{
    private const int PerfectFourthInSemitones = 5;

    private const int PerfectFifthInSemitones = 7;

    public double Score(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count == 0 || compositionConfiguration.Instruments.Count < 2)
        {
            return 0d;
        }

        var lastChord = precedingChords[^1];

        // The outer voices rely on Instruments being ordered by register: CompositionConfiguration.Instruments sorts
        // by descending MinNote, so index 0 is the highest-floor (soprano) voice and the last is the lowest (bass).
        var sopranoInstrument = compositionConfiguration.Instruments[0];
        var bassInstrument = compositionConfiguration.Instruments[^1];

        if (!lastChord.ContainsInstrument(sopranoInstrument) ||
            !lastChord.ContainsInstrument(bassInstrument) ||
            !nextChord.ContainsInstrument(sopranoInstrument) ||
            !nextChord.ContainsInstrument(bassInstrument))
        {
            return 0d;
        }

        var sopranoMotion = NoteMotionExtensions.FromNotes(lastChord[sopranoInstrument], nextChord[sopranoInstrument]);
        var bassMotion = NoteMotionExtensions.FromNotes(lastChord[bassInstrument], nextChord[bassInstrument]);

        if (sopranoMotion == NoteMotion.Oblique || sopranoMotion != bassMotion)
        {
            return 0d;
        }

        var bassDistanceInSemitones = Math.Abs(nextChord[bassInstrument].NoteNumber - lastChord[bassInstrument].NoteNumber);

        return bassDistanceInSemitones is PerfectFourthInSemitones or PerfectFifthInSemitones ? 0d : 1d;
    }
}
