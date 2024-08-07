using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class MordentEighthNoteOrnamentationCleaner(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration
) : IProcessor<OrnamentationCleaningItem>
{
    private const int IndexToCheck = 1;

    public void Process(OrnamentationCleaningItem item)
    {
        if (!item.Note.Ornamentations[IndexToCheck].IsDissonantWith(item.OtherNote.Ornamentations[IndexToCheck]))
        {
            return;
        }

        var defaultTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.None, compositionConfiguration.Meter);

        if (item.Note.OrnamentationType == OrnamentationType.Mordent)
        {
            item.Note.ResetOrnamentation(defaultTimeSpan);
        }
        else
        {
            item.OtherNote.ResetOrnamentation(defaultTimeSpan);
        }
    }
}
