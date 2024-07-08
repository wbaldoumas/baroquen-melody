using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

internal sealed class ThirtySecondNoteOrnamentationCleaner : IOrnamentationCleaner
{
    private static readonly int[] OrnamentationIndicesToCheck = [1, 3, 5];

    public void Clean(BaroquenNote noteA, BaroquenNote noteB)
    {
        if (!OrnamentationIndicesToCheck.Any(index => noteA.Ornamentations[index].IsDissonantWith(noteB.Ornamentations[index])))
        {
            return;
        }

        if (noteA.Raw.NoteNumber > noteB.Raw.NoteNumber)
        {
            noteB.ResetOrnamentation();
        }
        else
        {
            noteA.ResetOrnamentation();
        }
    }
}
