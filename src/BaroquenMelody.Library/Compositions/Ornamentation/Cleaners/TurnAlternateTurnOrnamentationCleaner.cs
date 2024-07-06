using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

/// <summary>
///     Cleans conflicting ornamentations across two <see cref="BaroquenNote"/> objects, one with a turn and the other with an alternate turn.
///     When there is a conflict, the alternate turn ornamentation is removed.
/// </summary>
internal sealed class TurnAlternateTurnOrnamentationCleaner : IOrnamentationCleaner
{
    public void Clean(BaroquenNote noteA, BaroquenNote noteB)
    {
        if (noteA.OrnamentationType == OrnamentationType.Turn)
        {
            CleanTargetedNotes(noteA, noteB);
        }
        else
        {
            CleanTargetedNotes(noteB, noteA);
        }
    }

    private static void CleanTargetedNotes(BaroquenNote noteWithTurnOrnamentation, BaroquenNote noteWithAlternateTurnOrnamentation)
    {
        if (!noteWithTurnOrnamentation.Ornamentations[1].IsDissonantWith(noteWithAlternateTurnOrnamentation.Ornamentations[1]))
        {
            return;
        }

        noteWithAlternateTurnOrnamentation.ResetOrnamentation();
    }
}
