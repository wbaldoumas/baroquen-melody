﻿using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors;

internal sealed class PedalProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration,
    int pedalInterval
) : IProcessor<OrnamentationItem>
{
    public const int Interval = 2;

    public const int RootPedalInterval = -3;

    public const int ThirdPedalInterval = -2;

    public const int FifthPedalInterval = -4;

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var nextNote = item.NextBeat![item.Instrument];

        var currentNoteIndex = compositionConfiguration.Scale.IndexOf(currentNote);
        var nextNoteIndex = compositionConfiguration.Scale.IndexOf(nextNote);

        var isDescending = currentNoteIndex > nextNoteIndex;
        var notes = compositionConfiguration.Scale.GetNotes();

        var ornamentationNotes = new[]
        {
            notes[currentNoteIndex + pedalInterval],
            notes[isDescending ? currentNoteIndex - 1 : currentNoteIndex + 1],
            notes[currentNoteIndex + pedalInterval]
        };

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.Pedal, compositionConfiguration.Meter);

        var ornamentationTimeSpan = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.Pedal, compositionConfiguration.Meter);

        foreach (var note in ornamentationNotes)
        {
            currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Instrument, note, ornamentationTimeSpan));
        }

        currentNote.OrnamentationType = OrnamentationType.Pedal;
    }
}
