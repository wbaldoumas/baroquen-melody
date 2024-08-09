using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.ThreeFour;

internal sealed class DelayedRunEighthOrnamentationCleaner(CompositionConfiguration compositionConfiguration) : IProcessor<OrnamentationCleaningItem>
{
    private static readonly (int DelayedRunIndex, int EighthNoteIndex)[] _indicesToCheck = [(1, 0), (2, 1), (3, 2)];

    public void Process(OrnamentationCleaningItem item)
    {
        if (item.Note.OrnamentationType is OrnamentationType.DelayedRun)
        {
            Clean(item.Note, item.OtherNote);
        }
        else
        {
            Clean(item.OtherNote, item.Note);
        }
    }

    private void Clean(BaroquenNote noteWithDelayedRun, BaroquenNote noteWithEighthNotes)
    {
        if (!_indicesToCheck.Any(i => noteWithDelayedRun.Ornamentations[i.DelayedRunIndex].IsDissonantWith(noteWithEighthNotes.Ornamentations[i.EighthNoteIndex])))
        {
            return;
        }

        if (noteWithEighthNotes > noteWithDelayedRun)
        {
            noteWithDelayedRun.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
        }
        else
        {
            noteWithEighthNotes.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
        }
    }
}
