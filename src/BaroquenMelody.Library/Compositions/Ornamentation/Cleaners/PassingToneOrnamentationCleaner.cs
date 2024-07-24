using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

/// <inheritdoc cref="IOrnamentationCleaner"/>
internal sealed class PassingToneOrnamentationCleaner : IOrnamentationCleaner
{
    public void Clean(BaroquenNote noteA, BaroquenNote noteB)
    {
        if (!noteA.Ornamentations[0].IsDissonantWith(noteB.Ornamentations[0]))
        {
            return;
        }

        switch (noteA.OrnamentationType, noteB.OrnamentationType)
        {
            case (OrnamentationType.PassingTone, OrnamentationType.PassingTone) when noteA.Raw.NoteNumber > noteB.Raw.NoteNumber:
            case (OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone) when noteA.Raw.NoteNumber > noteB.Raw.NoteNumber:
            case (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone) when noteA.Raw.NoteNumber > noteB.Raw.NoteNumber:
            case (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone) when noteA.Raw.NoteNumber > noteB.Raw.NoteNumber:
            case (OrnamentationType.NeighborTone, OrnamentationType.NeighborTone) when noteA.Raw.NoteNumber > noteB.Raw.NoteNumber:
            case (OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone):
            case (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPassingTone):
            case (OrnamentationType.PassingTone, OrnamentationType.RepeatedEighthNote):
            case (OrnamentationType.DoublePassingTone, OrnamentationType.RepeatedEighthNote):
            case (OrnamentationType.DelayedPassingTone, OrnamentationType.RepeatedDottedEighthSixteenth):
            case (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.RepeatedDottedEighthSixteenth):
            case (OrnamentationType.DelayedPassingTone, OrnamentationType.NeighborTone):
            case (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.NeighborTone):
            case (OrnamentationType.NeighborTone, OrnamentationType.RepeatedDottedEighthSixteenth):
                noteB.ResetOrnamentation();
                break;
            case (OrnamentationType.PassingTone, OrnamentationType.PassingTone) when noteA.Raw.NoteNumber < noteB.Raw.NoteNumber:
            case (OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone) when noteA.Raw.NoteNumber < noteB.Raw.NoteNumber:
            case (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone) when noteA.Raw.NoteNumber < noteB.Raw.NoteNumber:
            case (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone) when noteA.Raw.NoteNumber < noteB.Raw.NoteNumber:
            case (OrnamentationType.NeighborTone, OrnamentationType.NeighborTone) when noteA.Raw.NoteNumber < noteB.Raw.NoteNumber:
            case (OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone):
            case (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedDoublePassingTone):
            case (OrnamentationType.RepeatedEighthNote, OrnamentationType.PassingTone):
            case (OrnamentationType.RepeatedEighthNote, OrnamentationType.DoublePassingTone):
            case (OrnamentationType.RepeatedDottedEighthSixteenth, OrnamentationType.DelayedPassingTone):
            case (OrnamentationType.RepeatedDottedEighthSixteenth, OrnamentationType.DelayedDoublePassingTone):
            case (OrnamentationType.NeighborTone, OrnamentationType.DelayedPassingTone):
            case (OrnamentationType.NeighborTone, OrnamentationType.DelayedDoublePassingTone):
            case (OrnamentationType.RepeatedDottedEighthSixteenth, OrnamentationType.NeighborTone):
                noteA.ResetOrnamentation();
                break;
            default:
                if (noteA.Raw.NoteNumber > noteB.Raw.NoteNumber)
                {
                    noteB.ResetOrnamentation();
                }
                else
                {
                    noteA.ResetOrnamentation();
                }

                break;
        }
    }
}
