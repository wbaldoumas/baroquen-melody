using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

internal sealed class ThirtySecondNoteRunProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration configuration
) : IProcessor<OrnamentationItem>
{
    public const int Interval = 5;

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var nextNote = item.NextBeat![item.Voice];

        var notes = configuration.Scale.GetNotes();

        var currentNoteIndex = notes.IndexOf(currentNote.Raw);
        var nextNoteIndex = notes.IndexOf(nextNote.Raw);

        var isDescending = currentNoteIndex > nextNoteIndex;

        var ornamentationNotes = new List<Note>
        {
            notes[isDescending ? currentNoteIndex - 1 : currentNoteIndex + 1],
            notes[isDescending ? currentNoteIndex - 2 : currentNoteIndex + 2],
            notes[isDescending ? currentNoteIndex - 3 : currentNoteIndex + 3],
            notes[isDescending ? currentNoteIndex - 1 : currentNoteIndex + 1],
            notes[isDescending ? currentNoteIndex - 2 : currentNoteIndex + 2],
            notes[isDescending ? currentNoteIndex - 3 : currentNoteIndex + 3],
            notes[isDescending ? currentNoteIndex - 4 : currentNoteIndex + 4]
        };

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.ThirtySecondNoteRun, configuration.Meter);

        var ornamentationDuration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.ThirtySecondNoteRun, configuration.Meter);

        foreach (var note in ornamentationNotes)
        {
            currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, note)
            {
                Duration = ornamentationDuration
            });
        }

        currentNote.OrnamentationType = OrnamentationType.ThirtySecondNoteRun;
    }
}
