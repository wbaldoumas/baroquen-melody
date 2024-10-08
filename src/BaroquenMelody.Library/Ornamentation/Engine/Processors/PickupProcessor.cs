﻿using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors;

internal sealed class PickupProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration,
    OrnamentationType ornamentationType
) : IProcessor<OrnamentationItem>
{
    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var nextNote = item.NextBeat![item.Instrument];
        var nextNoteIndex = compositionConfiguration.Scale.IndexOf(nextNote);
        var notes = compositionConfiguration.Scale.GetNotes();
        var pickupNote = notes[currentNote > nextNote ? nextNoteIndex - 1 : nextNoteIndex + 1];

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, compositionConfiguration.Meter);

        currentNote.Ornamentations.Add(
            new BaroquenNote(
                item.Instrument,
                pickupNote,
                musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, compositionConfiguration.Meter)
            )
        );

        currentNote.OrnamentationType = ornamentationType;
    }
}
