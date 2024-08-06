using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class MordentSixteenthNoteOrnamentationCleaner : IProcessor<OrnamentationCleaningItem>
{
    private const int IndexToCheck = 1;

    public void Process(OrnamentationCleaningItem item)
    {
        if (!item.Note.Ornamentations[IndexToCheck].IsDissonantWith(item.OtherNote.Ornamentations[IndexToCheck]))
        {
            return;
        }

        if (item.Note.OrnamentationType == OrnamentationType.Mordent)
        {
            item.Note.ResetOrnamentation();
        }
        else
        {
            item.OtherNote.ResetOrnamentation();
        }
    }
}
