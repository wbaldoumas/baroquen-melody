using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Motifs;

/// <inheritdoc cref="IMotifBankFactory"/>
internal sealed class MotifBankFactory(
    IMotifExtractor motifExtractor,
    CompositionConfiguration compositionConfiguration
) : IMotifBankFactory
{
    /// <inheritdoc/>
    public MotifBank Create(BaroquenTheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);

        var scale = compositionConfiguration.Scale;
        var motifsByInstrument = new Dictionary<Instrument, AnchoredMotif>();

        foreach (var instrument in compositionConfiguration.Instruments)
        {
            var voiceLine = ExtractEntryVoiceLine(theme.Exposition, instrument);

            if (voiceLine.Count == 0)
            {
                continue;
            }

            var motif = motifExtractor.Extract(voiceLine);
            var anchorScaleIndex = scale.IndexOf(voiceLine[0]);

            motifsByInstrument[instrument] = new AnchoredMotif(motif, anchorScaleIndex, instrument);
        }

        return new MotifBank(motifsByInstrument);
    }

    // A voice's motif is its subject/answer entry: the FIRST exposition measure that voice appears in, read as that
    // instrument's per-beat principal line. The exposition's staggered, instrument-stripped layout guarantees this is
    // the clean entry rather than an accompaniment fragment.
    private static List<BaroquenNote> ExtractEntryVoiceLine(List<Measure> measures, Instrument instrument)
    {
        foreach (var measure in measures)
        {
            var entryNotes = measure.Beats
                .Where(beat => beat.ContainsInstrument(instrument))
                .Select(beat => beat[instrument])
                .ToList();

            if (entryNotes.Count > 0)
            {
                return entryNotes;
            }
        }

        return [];
    }
}
