using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors;

internal sealed class DoubleRunProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration
) : IProcessor<OrnamentationItem>
{
    public const int Interval = 5;

    private readonly int[] _ornamentationTranslations = [1, 2, 3, 1, 2, 3, 4];

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var nextNote = item.NextBeat![item.Instrument];

        var currentNoteIndex = compositionConfiguration.Scale.IndexOf(currentNote);
        var nextNoteIndex = compositionConfiguration.Scale.IndexOf(nextNote);

        var isDescending = currentNoteIndex > nextNoteIndex;
        var notes = compositionConfiguration.Scale.GetNotes();

        var ornamentationNotes = _ornamentationTranslations
            .Select(ornamentationTranslation => isDescending ? currentNoteIndex - ornamentationTranslation : currentNoteIndex + ornamentationTranslation)
            .Select(index => notes[index]);

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.DoubleRun, compositionConfiguration.Meter);

        var ornamentationTimeSpan = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.DoubleRun, compositionConfiguration.Meter);

        foreach (var note in ornamentationNotes)
        {
            currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Instrument, note, ornamentationTimeSpan));
        }

        currentNote.OrnamentationType = OrnamentationType.DoubleRun;
    }
}
