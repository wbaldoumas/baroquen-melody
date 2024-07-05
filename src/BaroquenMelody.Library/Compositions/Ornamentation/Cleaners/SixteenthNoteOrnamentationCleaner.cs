using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

/// <summary>
///     A cleaner that removes conflicting ornamentations across two <see cref="BaroquenNote"/> objects. At the moment, this cleaner
///     only removes conflicting ornamentations for the second ornamentation index (which is the "strong" note in the ornamentation).
/// </summary>
internal sealed class SixteenthNoteOrnamentationCleaner : IOrnamentationCleaner
{
    private const int OrnamentationIndexToCheck = 1;

    public void Clean(BaroquenNote noteA, BaroquenNote noteB)
    {
        if (!noteA.Ornamentations[OrnamentationIndexToCheck].IsDissonantWith(noteB.Ornamentations[OrnamentationIndexToCheck]))
        {
            return;
        }

        if (noteA.Raw.NoteNumber > noteB.Raw.NoteNumber)
        {
            noteB.ResetOrnamentation();
        }
        else
        {
            noteA.ResetOrnamentation();
        }
    }
}
