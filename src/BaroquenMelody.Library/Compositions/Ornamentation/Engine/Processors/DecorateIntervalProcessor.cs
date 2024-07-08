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
    public void Process(OrnamentationItem item)
    {
        Console.WriteLine("DecorateIntervalProcessor.Process");

        var currentNote = item.CurrentBeat[item.Voice];
        var notes = compositionConfiguration.Scale.GetNotes();
        var noteIndex = notes.IndexOf(currentNote.Raw);
        var firstNote = notes[noteIndex + intervalChange];
        var secondNote = notes[noteIndex + intervalChange - 1];
        var thirdNote = notes[noteIndex + intervalChange];

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.DecorateInterval, compositionConfiguration.Meter);

        var ornamentationDuration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.DecorateInterval, compositionConfiguration.Meter);

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, firstNote)
        {
            Duration = ornamentationDuration
        });

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, secondNote)
        {
            Duration = ornamentationDuration
        });

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, thirdNote)
        {
            Duration = ornamentationDuration
        });

        currentNote.OrnamentationType = OrnamentationType.DecorateInterval;
    }
}
