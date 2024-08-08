using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Extensions;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class SixteenthNoteOrnamentationCleaner(CompositionConfiguration compositionConfiguration) : IProcessor<OrnamentationCleaningItem>
{
    private static readonly int[] IndicesToCheck = [1, 3, 5];

    public void Process(OrnamentationCleaningItem item)
    {
        if (!IndicesToCheck.Any(index => item.Note.Ornamentations[index].IsDissonantWith(item.OtherNote.Ornamentations[index])))
        {
            return;
        }

        if (item.Note > item.OtherNote)
        {
            item.OtherNote.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
        }
        else
        {
            item.Note.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
        }
    }
}
