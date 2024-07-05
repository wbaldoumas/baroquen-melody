using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

/// <inheritdoc cref="IOrnamentationCleaner"/>
internal sealed class PassingToneOrnamentationCleaner : IOrnamentationCleaner
{
    public void Clean(BaroquenNote noteA, BaroquenNote noteB)
    {
        if (!noteA.Ornamentations[0].IsDissonantWith(noteB.Ornamentations[0]))
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
