﻿using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors;

internal sealed class PassingToneProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration,
    OrnamentationType ornamentationType
) : IProcessor<OrnamentationItem>
{
    public const int Interval = 2;

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var nextNote = item.NextBeat![item.Instrument];

        var currentNoteIndex = compositionConfiguration.Scale.IndexOf(currentNote);
        var nextNoteIndex = compositionConfiguration.Scale.IndexOf(nextNote);

        var notes = compositionConfiguration.Scale.GetNotes();
        var passingTone = notes[currentNoteIndex > nextNoteIndex ? currentNoteIndex - 1 : currentNoteIndex + 1];

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, compositionConfiguration.Meter);

        currentNote.Ornamentations.Add(
            new BaroquenNote(
                currentNote.Instrument,
                passingTone,
                musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, compositionConfiguration.Meter)
            )
        );

        currentNote.OrnamentationType = ornamentationType;
    }
}
