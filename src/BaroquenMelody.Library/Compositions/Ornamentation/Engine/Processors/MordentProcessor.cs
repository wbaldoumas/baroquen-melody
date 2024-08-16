using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

internal sealed class MordentProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    CompositionConfiguration compositionConfiguration
) : IProcessor<OrnamentationItem>
{
    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var currentNoteIndex = compositionConfiguration.Scale.IndexOf(currentNote);

        var firstNoteIndex = weightedRandomBooleanGenerator.IsTrue() ? currentNoteIndex + 1 : currentNoteIndex - 1;

        var notes = compositionConfiguration.Scale.GetNotes();

        var firstNote = notes[firstNoteIndex];
        var secondNote = notes[currentNoteIndex];

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.Mordent, compositionConfiguration.Meter);

        currentNote.Ornamentations.Add(
            new BaroquenNote(
                currentNote.Instrument,
                firstNote,
                musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.Mordent, compositionConfiguration.Meter)
            )
        );

        currentNote.Ornamentations.Add(
            new BaroquenNote(
                currentNote.Instrument,
                secondNote,
                musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.Mordent, compositionConfiguration.Meter, 2)
            )
        );

        currentNote.OrnamentationType = OrnamentationType.Mordent;
    }
}
