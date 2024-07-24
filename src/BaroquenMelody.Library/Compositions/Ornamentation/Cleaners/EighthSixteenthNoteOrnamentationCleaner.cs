using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

/// <summary>
///     A cleaner that removes conflicting ornamentations across two <see cref="BaroquenNote"/> objects, one with
///     eighth notes and the other with a sixteenth note run.
/// </summary>
internal sealed class EighthSixteenthNoteOrnamentationCleaner : IOrnamentationCleaner
{
    public void Clean(BaroquenNote noteA, BaroquenNote noteB)
    {
        if (noteA.OrnamentationType is OrnamentationType.PassingTone or OrnamentationType.DoublePassingTone or OrnamentationType.RepeatedEighthNote)
        {
            CleanTargetedNotes(noteA, noteB);
        }
        else
        {
            CleanTargetedNotes(noteB, noteA);
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
