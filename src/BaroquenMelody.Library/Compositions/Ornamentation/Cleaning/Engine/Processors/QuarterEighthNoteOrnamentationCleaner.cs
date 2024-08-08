using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class QuarterEighthNoteOrnamentationCleaner(CompositionConfiguration compositionConfiguration) : IProcessor<OrnamentationCleaningItem>
{
    private const int PassingToneIndexToCheck = 0;

    private const int EighthNoteIndexToCheck = 1;

    public void Process(OrnamentationCleaningItem item)
    {
        if (item.Note.OrnamentationType is OrnamentationType.PassingTone or OrnamentationType.DoublePassingTone or OrnamentationType.RepeatedNote)
        {
            CleanTargetedNotes(item.Note, item.OtherNote);
        }
        else
        {
            CleanTargetedNotes(item.OtherNote, item.Note);
        }
    }

    private void CleanTargetedNotes(BaroquenNote noteWithPassingToneOrnamentation, BaroquenNote noteWithEighthNotes)
    {
        if (!noteWithPassingToneOrnamentation.Ornamentations[PassingToneIndexToCheck].IsDissonantWith(noteWithEighthNotes.Ornamentations[EighthNoteIndexToCheck]))
        {
            return;
        }

        noteWithPassingToneOrnamentation.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
    }
}
