using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class EighthSixteenthNoteOrnamentationCleaner : IProcessor<OrnamentationCleaningItem>
{
    private const int PassingToneIndexToCheck = 0;

    private const int SixteenthNoteIndexToCheck = 1;

    public void Process(OrnamentationCleaningItem item)
    {
        if (item.Note.OrnamentationType is OrnamentationType.PassingTone or OrnamentationType.DoublePassingTone or OrnamentationType.RepeatedEighthNote)
        {
            CleanTargetedNotes(item.Note, item.OtherNote);
        }
        else
        {
            CleanTargetedNotes(item.OtherNote, item.Note);
        }
    }

    private static void CleanTargetedNotes(BaroquenNote noteWithPassingToneOrnamentation, BaroquenNote noteWithSixteenthNoteOrnamentation)
    {
        if (noteWithPassingToneOrnamentation.Ornamentations[PassingToneIndexToCheck].IsDissonantWith(noteWithSixteenthNoteOrnamentation.Ornamentations[SixteenthNoteIndexToCheck]))
        {
            noteWithPassingToneOrnamentation.ResetOrnamentation();
        }
    }
}
