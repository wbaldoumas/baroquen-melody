using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

/// <inheritdoc cref="IProcessor{T}"/>
internal sealed class NeighborToneProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    OrnamentationType ornamentationType,
    CompositionConfiguration compositionConfiguration
) : IProcessor<OrnamentationItem>
{
    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var currentNoteScaleIndex = compositionConfiguration.Scale.IndexOf(currentNote);

        var nextNoteScaleIndex = weightedRandomBooleanGenerator.IsTrue() ? currentNoteScaleIndex + 1 : currentNoteScaleIndex - 1;

        var notes = compositionConfiguration.Scale.GetNotes();
        var nextNote = notes[nextNoteScaleIndex];

        var ornamentationNote = new BaroquenNote(item.Instrument, nextNote, musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, compositionConfiguration.Meter));

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, compositionConfiguration.Meter);
        currentNote.OrnamentationType = ornamentationType;
        currentNote.Ornamentations.Add(ornamentationNote);
    }
}
