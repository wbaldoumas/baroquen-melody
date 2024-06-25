using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

internal sealed class SixteenthNoteRunProcessor(IMusicalTimeSpanCalculator musicalTimeSpanCalculator, CompositionConfiguration configuration) : IProcessor<OrnamentationItem>
{
    public const int Interval = 4;

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var nextNote = item.NextBeat![item.Voice];

        var notes = configuration.Scale.GetNotes().ToList();

        var currentNoteIndex = notes.IndexOf(currentNote.Raw);
        var nextNoteIndex = notes.IndexOf(nextNote.Raw);

        var firstNoteIndex = currentNoteIndex > nextNoteIndex ? currentNoteIndex - 1 : currentNoteIndex + 1;
        var secondNoteIndex = currentNoteIndex > nextNoteIndex ? currentNoteIndex - 2 : currentNoteIndex + 2;
        var thirdNoteIndex = currentNoteIndex > nextNoteIndex ? currentNoteIndex - 3 : currentNoteIndex + 3;

        var firstNote = notes[firstNoteIndex];
        var secondNote = notes[secondNoteIndex];
        var thirdNote = notes[thirdNoteIndex];

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.SixteenthNoteRun, configuration.Meter);

        var duration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.SixteenthNoteRun, configuration.Meter);

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, firstNote)
        {
            Duration = duration
        });

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, secondNote)
        {
            Duration = duration
        });

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, thirdNote)
        {
            Duration = duration
        });
    }
}
