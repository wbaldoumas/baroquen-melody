using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.ThreeFour;

internal sealed class HalfQuarterEighthOrnamentationCleaner(CompositionConfiguration compositionConfiguration) : IProcessor<OrnamentationCleaningItem>
{
    private static readonly (int QuarterNoteIndex, int EighthNoteIndex)[] _indicesToCheck = [(0, 1)];

    public void Process(OrnamentationCleaningItem item)
    {
        if (item.Note.OrnamentationType is
            OrnamentationType.PassingTone or
            OrnamentationType.DelayedDoublePassingTone or
            OrnamentationType.RepeatedNote or
            OrnamentationType.NeighborTone)
        {
            Clean(item.Note, item.OtherNote);
        }
        else
        {
            Clean(item.OtherNote, item.Note);
        }
    }

    private void Clean(BaroquenNote noteWithHalfQuarter, BaroquenNote noteWithEighths)
    {
        if (!_indicesToCheck.Any(i => noteWithHalfQuarter.Ornamentations[i.QuarterNoteIndex].IsDissonantWith(noteWithEighths.Ornamentations[i.EighthNoteIndex])))
        {
            return;
        }

        noteWithHalfQuarter.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
    }
}
