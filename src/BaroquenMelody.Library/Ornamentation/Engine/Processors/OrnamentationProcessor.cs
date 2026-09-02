using Atrea.PolicyEngine.Processors;
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

        if (!TryGetOrnamentations(currentNote, nextNote, out var ornamentations))
        {
            return;
        }

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(configuration.OrnamentationType, compositionConfiguration.Meter);

        foreach (var ornamentation in ornamentations)
        {
            currentNote.Ornamentations.Add(ornamentation);
        }

        currentNote.OrnamentationType = configuration.OrnamentationType;
    }

    private bool TryGetOrnamentations(BaroquenNote currentNote, BaroquenNote? nextNote, out List<BaroquenNote> ornamentations)
    {
        var shouldInvert = configuration.ShouldInvertTranslations((currentNote, nextNote));

        var translationPivot = configuration.ShouldTranslateOnCurrentNote
            ? compositionConfiguration.Scale.IndexOf(currentNote)
            : compositionConfiguration.Scale.IndexOf(nextNote!);

        var notes = compositionConfiguration.Scale.GetNotes();

        var noteIndices = configuration.Translations
            .Select((translation, translationIndex) => shouldInvert && configuration.TranslationInversionIndices.Contains(translationIndex)
                ? translationPivot - translation
                : translationPivot + translation
            )
            .ToList();

        // A translation that leaves the scale's note list means the ornamentation cannot be applied at this site.
        if (translationPivot < 0 || noteIndices.Exists(noteIndex => noteIndex < 0 || noteIndex >= notes.Count))
        {
            ornamentations = [];

            return false;
        }

        ornamentations = noteIndices
            .Select((noteIndex, ornamentationStep) =>
                new BaroquenNote(
                    currentNote.Instrument,
                    notes[noteIndex],
                    musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(configuration.OrnamentationType, compositionConfiguration.Meter, ornamentationStep)
                )
            )
            .ToList();

        return true;
    }
}
