using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class TurnAlternateTurnOrnamentationCleaner : IProcessor<OrnamentationCleaningItem>
{
    public void Process(OrnamentationCleaningItem item)
    {
        if (item.Note.OrnamentationType is OrnamentationType.Turn)
        {
            CleanTargetedNotes(item.Note, item.OtherNote);
        }
        else
        {
            CleanTargetedNotes(item.OtherNote, item.Note);
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
