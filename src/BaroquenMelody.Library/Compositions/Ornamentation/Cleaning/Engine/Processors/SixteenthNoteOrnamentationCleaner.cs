using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Extensions;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class SixteenthNoteOrnamentationCleaner : IProcessor<OrnamentationCleaningItem>
{
    public void Process(OrnamentationCleaningItem item)
    {
        if (!item.Note.Ornamentations[1].IsDissonantWith(item.OtherNote.Ornamentations[1]))
        {
            return;
        }

        if (item.Note.Raw.NoteNumber > item.OtherNote.Raw.NoteNumber)
        {
            item.OtherNote.ResetOrnamentation();
        }
        else
        {
            item.Note.ResetOrnamentation();
        }
    }
}
