using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

/// <inheritdoc cref="IProcessor{T}"/>
internal sealed class NeighborToneProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration
) : IProcessor<OrnamentationItem>
{
    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var notes = compositionConfiguration.Scale.GetNotes();
        var currentNoteScaleIndex = notes.IndexOf(currentNote.Raw);

        var nextNoteScaleIndex = WeightedRandomBooleanGenerator.Generate() ? currentNoteScaleIndex + 1 : currentNoteScaleIndex - 1;

        var nextNote = notes[nextNoteScaleIndex];

        var ornamentationNote = new BaroquenNote(item.Voice, nextNote)
        {
            Duration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.NeighborTone, compositionConfiguration.Meter)
        };

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.NeighborTone, compositionConfiguration.Meter);
        currentNote.OrnamentationType = OrnamentationType.NeighborTone;
        currentNote.Ornamentations.Add(ornamentationNote);
    }
}
