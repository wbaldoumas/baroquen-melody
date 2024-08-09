using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.ThreeFour;

internal sealed class DoublePassingToneQuarterOrnamentationCleaner(CompositionConfiguration compositionConfiguration) : IProcessor<OrnamentationCleaningItem>
{
    private const int DoublePassingToneIndexToCheck = 1;

    private const int HalfQuarterNoteIndexToCheck = 0;

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

    private void Clean(BaroquenNote noteWithDoublePassingTone, BaroquenNote noteWithHalfQuarter)
    {
        if (!noteWithDoublePassingTone.Ornamentations[DoublePassingToneIndexToCheck].IsDissonantWith(noteWithHalfQuarter.Ornamentations[HalfQuarterNoteIndexToCheck]))
        {
            return;
        }

        noteWithHalfQuarter.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
    }
}
