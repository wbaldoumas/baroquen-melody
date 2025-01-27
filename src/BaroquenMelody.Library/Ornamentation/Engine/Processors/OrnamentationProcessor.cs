﻿using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;
using BaroquenMelody.Library.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors;

internal sealed class OrnamentationProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration,
    OrnamentationProcessorConfiguration configuration
) : IProcessor<OrnamentationItem>
{
    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var nextNote = item.NextBeat?[item.Instrument];

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(configuration.OrnamentationType, compositionConfiguration.Meter);

        foreach (var ornamentation in GetOrnamentations(currentNote, nextNote))
        {
            currentNote.Ornamentations.Add(ornamentation);
        }

        currentNote.OrnamentationType = configuration.OrnamentationType;
    }

    private IEnumerable<BaroquenNote> GetOrnamentations(BaroquenNote currentNote, BaroquenNote? nextNote)
    {
        var shouldInvert = configuration.ShouldInvertTranslations((currentNote, nextNote));

        var translationPivot = configuration.ShouldTranslateOnCurrentNote
            ? compositionConfiguration.Scale.IndexOf(currentNote)
            : compositionConfiguration.Scale.IndexOf(nextNote!);

        var notes = compositionConfiguration.Scale.GetNotes();

        return configuration.Translations
            .Select((translation, translationIndex) => shouldInvert && configuration.TranslationInversionIndices.Contains(translationIndex)
                ? translationPivot - translation
                : translationPivot + translation
            )
            .Select(noteIndex => notes[noteIndex])
            .Select((note, ornamentationStep) =>
                new BaroquenNote(
                    currentNote.Instrument,
                    note,
                    musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(configuration.OrnamentationType, compositionConfiguration.Meter, ornamentationStep)
                )
            );
    }
}
