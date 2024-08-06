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
            Clean(item.Note, item.OtherNote);
        }
        else
        {
            Clean(item.OtherNote, item.Note);
        }
    }

    private static void Clean(BaroquenNote noteWithThirtySecondNotes, BaroquenNote noteWithSixteenthNotes)
    {
        if (!IndicesToCheck.Any(i => noteWithSixteenthNotes.Ornamentations[i.SixteenthNoteIndex].IsDissonantWith(noteWithThirtySecondNotes.Ornamentations[i.ThirtySecondNoteIndex])))
        {
            return;
        }

        if (noteWithSixteenthNotes > noteWithThirtySecondNotes)
        {
            noteWithThirtySecondNotes.ResetOrnamentation();
        }
        else
        {
            noteWithSixteenthNotes.ResetOrnamentation();
        }
    }
}
