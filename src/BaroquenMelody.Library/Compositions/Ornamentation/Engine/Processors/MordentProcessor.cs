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
        var currentNote = item.CurrentBeat[item.Voice];

        var notes = compositionConfiguration.Scale.GetNotes();

        var currentNoteIndex = notes.IndexOf(currentNote.Raw);

        var firstNoteIndex = weightedRandomBooleanGenerator.IsTrue() ? currentNoteIndex + 1 : currentNoteIndex - 1;

        // ReSharper disable once InlineTemporaryVariable
        var secondNoteIndex = currentNoteIndex;

        var firstNote = notes[firstNoteIndex];
        var secondNote = notes[secondNoteIndex];

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.Mordent, compositionConfiguration.Meter);

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, firstNote)
        {
            Duration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.Mordent, compositionConfiguration.Meter)
        });

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, secondNote)
        {
            Duration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.Mordent, compositionConfiguration.Meter, 2)
        });

        currentNote.OrnamentationType = OrnamentationType.Mordent;
    }
}
