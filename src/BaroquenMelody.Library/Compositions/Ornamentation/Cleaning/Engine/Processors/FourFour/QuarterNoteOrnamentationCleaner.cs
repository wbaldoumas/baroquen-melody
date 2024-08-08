using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.FourFour;

internal sealed class QuarterNoteOrnamentationCleaner(CompositionConfiguration compositionConfiguration) : IProcessor<OrnamentationCleaningItem>
{
    private const int IndexToCheck = 0;

    public void Process(OrnamentationCleaningItem item)
    {
        var note = item.Note;
        var otherNote = item.OtherNote;

        if (!note.Ornamentations[IndexToCheck].IsDissonantWith(otherNote.Ornamentations[IndexToCheck]))
        {
            return;
        }

        if (ShouldCleanNoteBasedOnOrnamentation(note, otherNote))
        {
            otherNote.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
        }
        else if (ShouldCleanNoteBasedOnOrnamentation(otherNote, note))
        {
            note.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
        }
        else
        {
            if (note > otherNote)
            {
                otherNote.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
            }
            else
            {
                note.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
            }
        }
    }

    private static bool ShouldCleanNoteBasedOnOrnamentation(BaroquenNote note, BaroquenNote otherNote) => (note.OrnamentationType, otherNote.OrnamentationType) switch
    {
        (OrnamentationType.PassingTone, OrnamentationType.PassingTone) when note > otherNote => true,
        (OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone) when note > otherNote => true,
        (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone) when note > otherNote => true,
        (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone) when note > otherNote => true,
        (OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedNeighborTone) when note > otherNote => true,
        (OrnamentationType.NeighborTone, OrnamentationType.NeighborTone) when note > otherNote => true,
        (OrnamentationType.PassingTone, OrnamentationType.NeighborTone) => true,
        (OrnamentationType.DoublePassingTone, OrnamentationType.NeighborTone) => true,
        (OrnamentationType.NeighborTone, OrnamentationType.RepeatedNote) => true,
        (OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone) => true,
        (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPassingTone) => true,
        (OrnamentationType.PassingTone, OrnamentationType.RepeatedNote) => true,
        (OrnamentationType.DoublePassingTone, OrnamentationType.RepeatedNote) => true,
        (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedRepeatedNote) => true,
        (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedRepeatedNote) => true,
        (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedNeighborTone) => true,
        (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedNeighborTone) => true,
        (OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedRepeatedNote) => true,
        _ => false
    };
}
