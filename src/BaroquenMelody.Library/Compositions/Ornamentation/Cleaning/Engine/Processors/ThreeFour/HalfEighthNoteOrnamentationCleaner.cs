using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.ThreeFour;

internal sealed class HalfEighthNoteOrnamentationCleaner(CompositionConfiguration compositionConfiguration) : IProcessor<OrnamentationCleaningItem>
{
    private const int IndexToCheck = 0;

    public void Process(OrnamentationCleaningItem item)
    {
        if (!item.Note.Ornamentations[IndexToCheck].IsDissonantWith(item.OtherNote.Ornamentations[IndexToCheck]))
        {
            return;
        }

        if (ShouldCleanNoteBasedOnOrnamentation(item.Note, item.OtherNote))
        {
            item.OtherNote.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
        }
        else if (ShouldCleanNoteBasedOnOrnamentation(item.OtherNote, item.Note))
        {
            item.Note.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
        }
        else if (item.Note > item.OtherNote)
        {
            item.OtherNote.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
        }
        else
        {
            item.Note.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
        }
    }

    private static bool ShouldCleanNoteBasedOnOrnamentation(BaroquenNote note, BaroquenNote otherNote) => (note.OrnamentationType, otherNote.OrnamentationType) switch
    {
        (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedRepeatedNote) => true,
        (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedNeighborTone) => true,
        (OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedRepeatedNote) => true,
        _ => false
    };
}
