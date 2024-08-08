using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.ThreeFour;

internal sealed class QuarterQuarterEighthOrnamentationCleaner(CompositionConfiguration compositionConfiguration) : IProcessor<OrnamentationCleaningItem>
{
    private static readonly (int QuarterNoteIndex, int EighthNoteIndex)[] _indicesToCheck = [(1, 1)];

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

    private void Clean(BaroquenNote noteWithQuarters, BaroquenNote noteWithEighths)
    {
        if (!_indicesToCheck.Any(i => noteWithQuarters.Ornamentations[i.QuarterNoteIndex].IsDissonantWith(noteWithEighths.Ornamentations[i.EighthNoteIndex])))
        {
            return;
        }

        noteWithQuarters.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
    }
}
