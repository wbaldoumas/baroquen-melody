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
                noteB.ResetOrnamentation();
                break;
            case (OrnamentationType.PassingTone, OrnamentationType.PassingTone) when noteA.Raw.NoteNumber < noteB.Raw.NoteNumber:
                noteA.ResetOrnamentation();
                break;
            case (OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone) when noteA.Raw.NoteNumber > noteB.Raw.NoteNumber:
                noteB.ResetOrnamentation();
                break;
            case (OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone) when noteA.Raw.NoteNumber < noteB.Raw.NoteNumber:
                noteA.ResetOrnamentation();
                break;
            case (OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone):
                noteA.ResetOrnamentation();
                break;
            case (OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone):
                noteB.ResetOrnamentation();
                break;
            case (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone) when noteA.Raw.NoteNumber > noteB.Raw.NoteNumber:
                noteB.ResetOrnamentation();
                break;
            case (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone) when noteA.Raw.NoteNumber < noteB.Raw.NoteNumber:
                noteA.ResetOrnamentation();
                break;
            case (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone) when noteA.Raw.NoteNumber > noteB.Raw.NoteNumber:
                noteB.ResetOrnamentation();
                break;
            case (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone) when noteA.Raw.NoteNumber < noteB.Raw.NoteNumber:
                noteA.ResetOrnamentation();
                break;
            case (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedDoublePassingTone):
                noteA.ResetOrnamentation();
                break;
            case (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPassingTone):
                noteB.ResetOrnamentation();
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
