using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Motifs;

/// <inheritdoc cref="IMotifApplicator"/>
internal sealed class MotifApplicator(CompositionConfiguration compositionConfiguration) : IMotifApplicator
{
    /// <inheritdoc/>
    public IReadOnlyList<BaroquenNote> Apply(AnchoredMotif anchoredMotif)
    {
        ArgumentNullException.ThrowIfNull(anchoredMotif);

        var gestures = anchoredMotif.Motif.Gestures;
        var notes = compositionConfiguration.Scale.GetNotes();
        var instrumentConfiguration = compositionConfiguration.InstrumentConfigurationsByInstrument[anchoredMotif.Instrument];
        var (minScaleIndex, maxScaleIndex) = GetInstrumentScaleIndexWindow(notes, instrumentConfiguration);

        var voiceLine = new List<BaroquenNote>(gestures.Count);

        // From-anchor prefix sum: runningScaleIndex accumulates the RAW (unclamped) absolute index, so a transient
        // excursion past a range edge does not poison later notes; only the per-note lookup index is clamped. The head
        // delta is 0, so the first note lands exactly on the anchor.
        var runningScaleIndex = anchoredMotif.AnchorScaleIndex;

        foreach (var gesture in gestures)
        {
            runningScaleIndex += gesture.ScaleStepDelta;

            var clampedScaleIndex = Math.Clamp(runningScaleIndex, minScaleIndex, maxScaleIndex);

            voiceLine.Add(new BaroquenNote(anchoredMotif.Instrument, notes[clampedScaleIndex], gesture.Duration));
        }

        return voiceLine;
    }

    // Maps the instrument's playable range to an inclusive scale-index window. Comparing by NoteNumber keeps this
    // correct even if Min/MaxNote are chromatic. The while-loops always yield a valid in-bounds [min, max] with
    // min <= max (so Math.Clamp never throws), degrading safely even for a degenerate out-of-scale range.
    private static (int MinScaleIndex, int MaxScaleIndex) GetInstrumentScaleIndexWindow(List<Note> notes, InstrumentConfiguration instrumentConfiguration)
    {
        var minScaleIndex = 0;

        while (minScaleIndex < notes.Count - 1 && notes[minScaleIndex].NoteNumber < instrumentConfiguration.MinNote.NoteNumber)
        {
            ++minScaleIndex;
        }

        var maxScaleIndex = notes.Count - 1;

        while (maxScaleIndex > minScaleIndex && notes[maxScaleIndex].NoteNumber > instrumentConfiguration.MaxNote.NoteNumber)
        {
            --maxScaleIndex;
        }

        return (minScaleIndex, maxScaleIndex);
    }
}
