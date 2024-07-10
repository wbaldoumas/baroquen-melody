using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

internal sealed class MordentSixteenthNoteOrnamentationCleaner : IOrnamentationCleaner
{
    public void Clean(BaroquenNote noteA, BaroquenNote noteB)
    {
        if (noteA.OrnamentationType == OrnamentationType.Mordent)
        {
            CleanTargetedNotes(noteA, noteB);
        }
        else
        {
            CleanTargetedNotes(noteB, noteA);
        }
    }

    private static void CleanTargetedNotes(BaroquenNote noteWithMordent, BaroquenNote noteWithSixteenths)
    {
        if (noteWithMordent.Ornamentations[1].IsDissonantWith(noteWithSixteenths.Ornamentations[1]))
        {
            noteWithMordent.ResetOrnamentation();
        }
    }
}
