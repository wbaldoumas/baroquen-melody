using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

internal sealed class AlternateTurnProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration
) : IProcessor<OrnamentationItem>
{
    public const int Interval = 1;

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var nextNote = item.NextBeat![item.Voice];

        var notes = compositionConfiguration.Scale.GetNotes();

        var currentNoteIndex = notes.IndexOf(currentNote.Raw);
        var nextNoteIndex = notes.IndexOf(nextNote.Raw);

        var firstNoteIndex = currentNoteIndex > nextNoteIndex ? currentNoteIndex - 1 : currentNoteIndex + 1;
        var secondNoteIndex = currentNoteIndex > nextNoteIndex ? currentNoteIndex + 1 : currentNoteIndex - 1;

        var firstNote = notes[firstNoteIndex];
        var secondNote = notes[secondNoteIndex];
        var thirdNote = notes[currentNoteIndex];

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.AlternateTurn, compositionConfiguration.Meter);

        var duration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.AlternateTurn, compositionConfiguration.Meter);

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

        currentNote.OrnamentationType = OrnamentationType.AlternateTurn;
    }
}
