﻿using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Infrastructure.Random;

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
        var currentNote = item.CurrentBeat[item.Voice];
        var notes = compositionConfiguration.Scale.GetNotes();
        var currentNoteScaleIndex = notes.IndexOf(currentNote.Raw);

        var nextNoteScaleIndex = weightedRandomBooleanGenerator.IsTrue() ? currentNoteScaleIndex + 1 : currentNoteScaleIndex - 1;

        var nextNote = notes[nextNoteScaleIndex];

        var ornamentationNote = new BaroquenNote(item.Voice, nextNote)
        {
            Duration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, compositionConfiguration.Meter)
        };

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, compositionConfiguration.Meter);
        currentNote.OrnamentationType = ornamentationType;
        currentNote.Ornamentations.Add(ornamentationNote);
    }
}
