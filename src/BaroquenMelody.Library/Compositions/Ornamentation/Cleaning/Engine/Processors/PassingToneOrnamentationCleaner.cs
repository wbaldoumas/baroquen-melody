using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class PassingToneOrnamentationCleaner : IProcessor<OrnamentationCleaningItem>
{
    public void Process(OrnamentationCleaningItem item)
    {
        var note = item.Note;
        var otherNote = item.OtherNote;

        if (!note.Ornamentations[0].IsDissonantWith(otherNote.Ornamentations[0]))
        {
            return;
        }

        switch (note.OrnamentationType, otherNote.OrnamentationType)
        {
            case (OrnamentationType.PassingTone, OrnamentationType.PassingTone) when note.Raw.NoteNumber > otherNote.Raw.NoteNumber:
            case (OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone) when note.Raw.NoteNumber > otherNote.Raw.NoteNumber:
            case (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone) when note.Raw.NoteNumber > otherNote.Raw.NoteNumber:
            case (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone) when note.Raw.NoteNumber > otherNote.Raw.NoteNumber:
            case (OrnamentationType.NeighborTone, OrnamentationType.NeighborTone) when note.Raw.NoteNumber > otherNote.Raw.NoteNumber:
            case (OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone):
            case (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPassingTone):
            case (OrnamentationType.PassingTone, OrnamentationType.RepeatedEighthNote):
            case (OrnamentationType.DoublePassingTone, OrnamentationType.RepeatedEighthNote):
            case (OrnamentationType.DelayedPassingTone, OrnamentationType.RepeatedDottedEighthSixteenth):
            case (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.RepeatedDottedEighthSixteenth):
            case (OrnamentationType.DelayedPassingTone, OrnamentationType.NeighborTone):
            case (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.NeighborTone):
            case (OrnamentationType.NeighborTone, OrnamentationType.RepeatedDottedEighthSixteenth):
                otherNote.ResetOrnamentation();
                break;
            case (OrnamentationType.PassingTone, OrnamentationType.PassingTone) when note.Raw.NoteNumber < otherNote.Raw.NoteNumber:
            case (OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone) when note.Raw.NoteNumber < otherNote.Raw.NoteNumber:
            case (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone) when note.Raw.NoteNumber < otherNote.Raw.NoteNumber:
            case (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone) when note.Raw.NoteNumber < otherNote.Raw.NoteNumber:
            case (OrnamentationType.NeighborTone, OrnamentationType.NeighborTone) when note.Raw.NoteNumber < otherNote.Raw.NoteNumber:
            case (OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone):
            case (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedDoublePassingTone):
            case (OrnamentationType.RepeatedEighthNote, OrnamentationType.PassingTone):
            case (OrnamentationType.RepeatedEighthNote, OrnamentationType.DoublePassingTone):
            case (OrnamentationType.RepeatedDottedEighthSixteenth, OrnamentationType.DelayedPassingTone):
            case (OrnamentationType.RepeatedDottedEighthSixteenth, OrnamentationType.DelayedDoublePassingTone):
            case (OrnamentationType.NeighborTone, OrnamentationType.DelayedPassingTone):
            case (OrnamentationType.NeighborTone, OrnamentationType.DelayedDoublePassingTone):
            case (OrnamentationType.RepeatedDottedEighthSixteenth, OrnamentationType.NeighborTone):
                note.ResetOrnamentation();
                break;
            default:
                if (note.Raw.NoteNumber > otherNote.Raw.NoteNumber)
                {
                    otherNote.ResetOrnamentation();
                }
                else
                {
                    note.ResetOrnamentation();
                }

                break;
        }
    }
}
