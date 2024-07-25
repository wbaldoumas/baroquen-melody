using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class EighthSixteenthNoteOrnamentationCleaner : IProcessor<OrnamentationCleaningItem>
{
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
        if (noteWithPassingToneOrnamentation.Ornamentations[0].IsDissonantWith(noteWithSixteenthNoteOrnamentation.Ornamentations[1]))
        {
            noteWithPassingToneOrnamentation.ResetOrnamentation();
        }
    }
}
