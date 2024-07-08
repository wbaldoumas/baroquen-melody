using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

internal sealed class ThirtySecondSixteenthNoteOrnamentationCleaner : IOrnamentationCleaner
{
    private static readonly (int SixteenthNoteIndex, int ThirtySecondNoteIndex)[] IndicesToCheck = [(0, 1), (1, 3), (2, 5)];

    public void Clean(BaroquenNote noteA, BaroquenNote noteB)
    {
        if (noteA.OrnamentationType is OrnamentationType.DoubleTurn or OrnamentationType.ThirtySecondNoteRun)
        {
            CleanTargetedNotes(noteA, noteB);
        }
        else
        {
            CleanTargetedNotes(noteB, noteA);
        }
    }

    private static void CleanTargetedNotes(BaroquenNote noteWithThirtySecondNoteOrnamentation, BaroquenNote noteWithSixteenthNoteOrnamentation)
    {
        foreach (var (sixteenthNoteIndex, thirtySecondNoteIndex) in IndicesToCheck)
        {
            if (!noteWithSixteenthNoteOrnamentation.Ornamentations[sixteenthNoteIndex].IsDissonantWith(noteWithThirtySecondNoteOrnamentation.Ornamentations[thirtySecondNoteIndex]))
            {
                continue;
            }

            if (noteWithSixteenthNoteOrnamentation.Raw.NoteNumber > noteWithThirtySecondNoteOrnamentation.Raw.NoteNumber)
            {
                noteWithThirtySecondNoteOrnamentation.ResetOrnamentation();
            }
            else
            {
                noteWithSixteenthNoteOrnamentation.ResetOrnamentation();
            }

            return;
        }
    }
}
