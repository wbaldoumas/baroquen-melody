using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class TurnAlternateTurnOrnamentationCleaner : IProcessor<OrnamentationCleaningItem>
{
    private const int IndexToCheck = 1;

    public void Process(OrnamentationCleaningItem item)
    {
        if (item.Note.OrnamentationType is OrnamentationType.Turn)
        {
            Clean(item.Note, item.OtherNote);
        }
        else
        {
            Clean(item.OtherNote, item.Note);
        }
    }

    private static void Clean(BaroquenNote noteWithTurnOrnamentation, BaroquenNote noteWithAlternateTurnOrnamentation)
    {
        if (!noteWithTurnOrnamentation.Ornamentations[IndexToCheck].IsDissonantWith(noteWithAlternateTurnOrnamentation.Ornamentations[IndexToCheck]))
        {
            return;
        }

        noteWithAlternateTurnOrnamentation.ResetOrnamentation();
    }
}
