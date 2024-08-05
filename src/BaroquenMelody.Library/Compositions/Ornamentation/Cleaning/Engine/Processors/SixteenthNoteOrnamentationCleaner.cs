using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Extensions;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class SixteenthNoteOrnamentationCleaner : IProcessor<OrnamentationCleaningItem>
{
    private static readonly int[] _ornamentationIndices = [0, 1, 2];

    public void Process(OrnamentationCleaningItem item)
    {
        if (!_ornamentationIndices.Any(i => item.Note.Ornamentations[i].IsDissonantWith(item.OtherNote.Ornamentations[i])))
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
