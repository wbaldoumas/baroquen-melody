using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Extensions;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class SixteenthNoteOrnamentationCleaner : IProcessor<OrnamentationCleaningItem>
{
    private static readonly int[] _ornamentationIndices = [0, 1, 2];

    public void Process(OrnamentationCleaningItem item)
    {
        if (!_ornamentationIndices.Any(index => item.Note.Ornamentations[index].IsDissonantWith(item.OtherNote.Ornamentations[index])))
        {
            return;
        }

        if (item.Note > item.OtherNote)
        {
            item.OtherNote.ResetOrnamentation();
        }
        else
        {
            item.Note.ResetOrnamentation();
        }
    }
}
