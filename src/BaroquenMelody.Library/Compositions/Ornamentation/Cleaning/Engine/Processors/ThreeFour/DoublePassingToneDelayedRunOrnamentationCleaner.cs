using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.ThreeFour;

internal sealed class DoublePassingToneDelayedRunOrnamentationCleaner(CompositionConfiguration compositionConfiguration) : IProcessor<OrnamentationCleaningItem>
{
    private static readonly (int PassingToneIndex, int DelayedRunIndex)[] _indicesToCheck = [(0, 0), (1, 2)];

    public void Process(OrnamentationCleaningItem item)
    {
        if (item.Note.OrnamentationType is OrnamentationType.DoublePassingTone)
        {
            Clean(item.Note, item.OtherNote);
        }
        else
        {
            Clean(item.OtherNote, item.Note);
        }
    }

    private void Clean(BaroquenNote noteWithPassingTone, BaroquenNote noteWithDelayedRun)
    {
        if (!_indicesToCheck.Any(i => noteWithPassingTone.Ornamentations[i.PassingToneIndex].IsDissonantWith(noteWithDelayedRun.Ornamentations[i.DelayedRunIndex])))
        {
            return;
        }

        noteWithPassingTone.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
    }
}
