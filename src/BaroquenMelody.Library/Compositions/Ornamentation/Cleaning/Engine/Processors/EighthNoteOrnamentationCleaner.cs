using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class EighthNoteOrnamentationCleaner : IProcessor<OrnamentationCleaningItem>
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
            otherNote.ResetOrnamentation();
        }
        else if (ShouldCleanNoteBasedOnOrnamentation(otherNote, note))
        {
            note.ResetOrnamentation();
        }
        else
        {
            if (note > otherNote)
            {
                otherNote.ResetOrnamentation();
            }
            else
            {
                note.ResetOrnamentation();
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
        (OrnamentationType.NeighborTone, OrnamentationType.RepeatedEighthNote) => true,
        (OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone) => true,
        (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPassingTone) => true,
        (OrnamentationType.PassingTone, OrnamentationType.RepeatedEighthNote) => true,
        (OrnamentationType.DoublePassingTone, OrnamentationType.RepeatedEighthNote) => true,
        (OrnamentationType.DelayedPassingTone, OrnamentationType.RepeatedDottedEighthSixteenth) => true,
        (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.RepeatedDottedEighthSixteenth) => true,
        (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedNeighborTone) => true,
        (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedNeighborTone) => true,
        (OrnamentationType.DelayedNeighborTone, OrnamentationType.RepeatedDottedEighthSixteenth) => true,
        _ => false
    };
}
