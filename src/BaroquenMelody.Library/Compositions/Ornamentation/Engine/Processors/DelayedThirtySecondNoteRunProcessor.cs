using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

internal sealed class DelayedThirtySecondNoteRunProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration
) : IProcessor<OrnamentationItem>
{
    public const int Interval = 5;

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var nextNote = item.NextBeat![item.Voice];

        var notes = compositionConfiguration.Scale.GetNotes();

        var currentNoteIndex = notes.IndexOf(currentNote.Raw);
        var nextNoteIndex = notes.IndexOf(nextNote.Raw);

        var firstNoteIndex = currentNoteIndex > nextNoteIndex ? currentNoteIndex - 1 : currentNoteIndex + 1;
        var secondNoteIndex = currentNoteIndex > nextNoteIndex ? currentNoteIndex - 2 : currentNoteIndex + 2;
        var thirdNoteIndex = currentNoteIndex > nextNoteIndex ? currentNoteIndex - 3 : currentNoteIndex + 3;
        var fourthNoteIndex = currentNoteIndex > nextNoteIndex ? currentNoteIndex - 4 : currentNoteIndex + 4;

        var firstNote = notes[firstNoteIndex];
        var secondNote = notes[secondNoteIndex];
        var thirdNote = notes[thirdNoteIndex];
        var fourthNote = notes[fourthNoteIndex];

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.DelayedThirtySecondNoteRun, compositionConfiguration.Meter);

        var ornamentationDuration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.DelayedThirtySecondNoteRun, compositionConfiguration.Meter);

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

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, fourthNote)
        {
            Duration = ornamentationDuration
        });

        currentNote.OrnamentationType = OrnamentationType.DelayedThirtySecondNoteRun;
    }
}
