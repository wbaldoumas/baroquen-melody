using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class ThirtySecondSixteenthNoteOrnamentationCleaner : IProcessor<OrnamentationCleaningItem>
{
    private static readonly (int SixteenthNoteIndex, int ThirtySecondNoteIndex)[] IndicesToCheck = [(0, 1), (1, 3), (2, 5)];

    public void Process(OrnamentationCleaningItem item)
    {
        if (item.Note.OrnamentationType is OrnamentationType.DoubleTurn or OrnamentationType.ThirtySecondNoteRun)
        {
            CleanTargetedNotes(item.Note, item.OtherNote);
        }
        else
        {
            CleanTargetedNotes(item.OtherNote, item.Note);
        }
    }

    private static void CleanTargetedNotes(BaroquenNote noteWithThirtySecondNoteOrnamentation, BaroquenNote noteWithSixteenthNoteOrnamentation)
    {
        foreach (var (sixteenthNoteIndex, thirtySecondNoteIndex) in IndicesToCheck)
        {
            if (!noteWithSixteenthNoteOrnamentation.Ornamentations[sixteenthNoteIndex].IsDissonantWith(noteWithThirtySecondNoteOrnamentation.Ornamentations[thirtySecondNoteIndex]))
            {
                continue;
            }

            if (noteWithSixteenthNoteOrnamentation.Raw.NoteNumber > noteWithThirtySecondNoteOrnamentation.Raw.NoteNumber)
            {
                noteWithThirtySecondNoteOrnamentation.ResetOrnamentation();
            }
            else
            {
                noteWithSixteenthNoteOrnamentation.ResetOrnamentation();
            }

            return;
        }
    }
}
