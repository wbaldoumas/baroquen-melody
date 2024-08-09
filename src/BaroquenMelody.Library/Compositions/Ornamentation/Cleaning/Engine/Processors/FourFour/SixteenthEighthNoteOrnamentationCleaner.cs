using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.FourFour;

internal sealed class SixteenthEighthNoteOrnamentationCleaner(CompositionConfiguration compositionConfiguration) : IProcessor<OrnamentationCleaningItem>
{
    private static readonly (int EighthNoteIndex, int SixteenthNoteIndex)[] IndicesToCheck = [(0, 1), (1, 3), (2, 5)];

    public void Process(OrnamentationCleaningItem item)
    {
        if (item.Note.OrnamentationType is OrnamentationType.DoubleTurn or OrnamentationType.DoubleRun)
        {
            Clean(item.Note, item.OtherNote);
        }
        else
        {
            Clean(item.OtherNote, item.Note);
        }
    }

    private void Clean(BaroquenNote noteWithSixteenthNotes, BaroquenNote noteWithEighthNotes)
    {
        if (!IndicesToCheck.Any(i => noteWithEighthNotes.Ornamentations[i.EighthNoteIndex].IsDissonantWith(noteWithSixteenthNotes.Ornamentations[i.SixteenthNoteIndex])))
        {
            return;
        }

        if (noteWithEighthNotes > noteWithSixteenthNotes)
        {
            noteWithSixteenthNotes.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
        }
        else
        {
            noteWithEighthNotes.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
        }
    }
}
