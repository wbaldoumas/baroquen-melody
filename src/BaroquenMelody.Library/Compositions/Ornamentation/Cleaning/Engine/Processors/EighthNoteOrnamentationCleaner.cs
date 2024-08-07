using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class EighthNoteOrnamentationCleaner(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration
) : IProcessor<OrnamentationCleaningItem>
{
    private static readonly int[] _ornamentationIndices = [0, 1, 2];

    public void Process(OrnamentationCleaningItem item)
    {
        if (!_ornamentationIndices.Any(index => item.Note.Ornamentations[index].IsDissonantWith(item.OtherNote.Ornamentations[index])))
        {
            return;
        }

        var defaultTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.None, compositionConfiguration.Meter);

        if (item.Note > item.OtherNote)
        {
            item.OtherNote.ResetOrnamentation(defaultTimeSpan);
        }
        else
        {
            item.Note.ResetOrnamentation(defaultTimeSpan);
        }
    }
}
