using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors;

internal sealed class InvertedTurnProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration
) : IProcessor<OrnamentationItem>
{
    public const int Interval = 1;

    private readonly int[] _ornamentationTranslations = [1, -1, 0];

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

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.InvertedTurn, compositionConfiguration.Meter);

        var ornamentationTimeSpan = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.InvertedTurn, compositionConfiguration.Meter);

        foreach (var ornamentation in ornamentationNotes)
        {
            currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Instrument, ornamentation, ornamentationTimeSpan));
        }

        currentNote.OrnamentationType = OrnamentationType.InvertedTurn;
    }
}
