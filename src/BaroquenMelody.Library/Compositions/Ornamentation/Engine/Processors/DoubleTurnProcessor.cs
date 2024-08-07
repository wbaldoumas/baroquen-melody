﻿using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

internal sealed class DoubleTurnProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration
) : IProcessor<OrnamentationItem>
{
    public const int Interval = 2;

    private readonly int[] _ornamentationTranslations = [1, -1, 0, 1, 2, 0, 1];

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var nextNote = item.NextBeat![item.Voice];

        var currentNoteIndex = compositionConfiguration.Scale.IndexOf(currentNote);
        var nextNoteIndex = compositionConfiguration.Scale.IndexOf(nextNote);

        var isDescending = currentNoteIndex > nextNoteIndex;
        var notes = compositionConfiguration.Scale.GetNotes();

        var ornamentationNotes = _ornamentationTranslations
            .Select(ornamentationTranslation => isDescending ? currentNoteIndex - ornamentationTranslation : currentNoteIndex + ornamentationTranslation)
            .Select(index => notes[index]);

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.DoubleTurn, compositionConfiguration.Meter);

        var ornamentationTimeSpan = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.DoubleTurn, compositionConfiguration.Meter);

        foreach (var ornamentation in ornamentationNotes)
        {
            currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, ornamentation, ornamentationTimeSpan));
        }

        currentNote.OrnamentationType = OrnamentationType.DoubleTurn;
    }
}
