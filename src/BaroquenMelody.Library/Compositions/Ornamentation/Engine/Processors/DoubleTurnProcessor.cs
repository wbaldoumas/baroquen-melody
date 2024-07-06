// ReSharper disable InlineTemporaryVariable

using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

internal sealed class DoubleTurnProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration
) : IProcessor<OrnamentationItem>
{
    public const int Interval = 2;

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var nextNote = item.NextBeat![item.Voice];

        var notes = compositionConfiguration.Scale.GetNotes();

        var currentNoteIndex = notes.IndexOf(currentNote.Raw);
        var nextNoteIndex = notes.IndexOf(nextNote.Raw);

        var isDescending = currentNoteIndex > nextNoteIndex;

        var firstNoteIndex = isDescending ? currentNoteIndex - 1 : currentNoteIndex + 1;
        var secondNoteIndex = isDescending ? currentNoteIndex + 1 : currentNoteIndex - 1;
        var thirdNoteIndex = currentNoteIndex;
        var fourthNoteIndex = isDescending ? currentNoteIndex - 1 : currentNoteIndex + 1;
        var fifthNoteIndex = isDescending ? currentNoteIndex - 2 : currentNoteIndex + 2;
        var sixthNoteIndex = currentNoteIndex;
        var seventhNoteIndex = isDescending ? currentNoteIndex - 1 : currentNoteIndex + 1;

        var firstNote = notes[firstNoteIndex];
        var secondNote = notes[secondNoteIndex];
        var thirdNote = notes[thirdNoteIndex];
        var fourthNote = notes[fourthNoteIndex];
        var fifthNote = notes[fifthNoteIndex];
        var sixthNote = notes[sixthNoteIndex];
        var seventhNote = notes[seventhNoteIndex];

        var ornamentations = new List<Note> { firstNote, secondNote, thirdNote, fourthNote, fifthNote, sixthNote, seventhNote };

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.DoubleTurn, compositionConfiguration.Meter);

        var ornamentationDuration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.DoubleTurn, compositionConfiguration.Meter);

        foreach (var ornamentation in ornamentations)
        {
            currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, ornamentation)
            {
                Duration = ornamentationDuration
            });
        }

        currentNote.OrnamentationType = OrnamentationType.DoubleTurn;
    }
}
