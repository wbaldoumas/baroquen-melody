using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Configuration;

namespace BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Processors;

/// <summary>
///     A generic ornamentation cleaner that can be used to clean any ornamentation.
/// </summary>
internal sealed class OrnamentationCleaner(
    OrnamentationCleanerConfiguration configuration,
    CompositionConfiguration compositionConfiguration,
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator
) : IProcessor<OrnamentationCleaningItem>
{
    private const int ChanceOfCleaningWeakConflicts = 25;

    public void Process(OrnamentationCleaningItem item)
    {
        var notePair = configuration.NotePairSelector.Select(item);

        var hasStrongPulseConflicts = configuration.NoteIndexPairs
            .Where(noteIndexPair => noteIndexPair.OccursOnStrongPulse)
            .Any(noteIndexPair => notePair.PrimaryNote.Ornamentations[noteIndexPair.PrimaryNoteIndex].IsDissonantWith(notePair.SecondaryNote.Ornamentations[noteIndexPair.SecondaryNoteIndex]));

        if (hasStrongPulseConflicts)
        {
            CleanConflictingOrnamentation(item);

            return;
        }

        var hasWeakPulseConflicts = configuration.NoteIndexPairs
            .Where(noteIndexPair => !noteIndexPair.OccursOnStrongPulse)
            .Any(noteIndexPair => notePair.PrimaryNote.Ornamentations[noteIndexPair.PrimaryNoteIndex].IsDissonantWith(notePair.SecondaryNote.Ornamentations[noteIndexPair.SecondaryNoteIndex]));

        if (hasWeakPulseConflicts && weightedRandomBooleanGenerator.IsTrue(ChanceOfCleaningWeakConflicts))
        {
            CleanConflictingOrnamentation(item);
        }
    }

    private void CleanConflictingOrnamentation(OrnamentationCleaningItem item)
    {
        var noteToClean = configuration.NoteTargetSelector.Select(item);

        noteToClean.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
    }
}
