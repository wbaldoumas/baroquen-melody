using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class MordentSixteenthNoteOrnamentationCleaner : IProcessor<OrnamentationCleaningItem>
{
    public void Process(OrnamentationCleaningItem item)
    {
        if (item.Note.OrnamentationType == OrnamentationType.Mordent)
        {
            CleanTargetedNotes(item.Note, item.OtherNote);
        }
        else
        {
            CleanTargetedNotes(item.OtherNote, item.Note);
        }
    }

    private static void CleanTargetedNotes(BaroquenNote noteWithMordent, BaroquenNote noteWithSixteenths)
    {
        if (noteWithMordent.Ornamentations[1].IsDissonantWith(noteWithSixteenths.Ornamentations[1]))
        {
            noteWithMordent.ResetOrnamentation();
        }
    }
}
