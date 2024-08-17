using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

internal sealed class DecorateIntervalProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration,
    int intervalChange
) : IProcessor<OrnamentationItem>
{
    private readonly int[] _ornamentationTranslations = [intervalChange, intervalChange - 1, intervalChange];

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var currentNoteIndex = compositionConfiguration.Scale.IndexOf(currentNote);
        var notes = compositionConfiguration.Scale.GetNotes();

        var ornamentationNotes = _ornamentationTranslations
            .Select(ornamentationTranslation => currentNoteIndex + ornamentationTranslation)
            .Select(index => notes[index]);

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.DecorateInterval, compositionConfiguration.Meter);

        var ornamentationTimeSpan = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.DecorateInterval, compositionConfiguration.Meter);

        foreach (var ornamentation in ornamentationNotes)
        {
            currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Instrument, ornamentation, ornamentationTimeSpan));
        }

        currentNote.OrnamentationType = OrnamentationType.DecorateInterval;
    }
}
