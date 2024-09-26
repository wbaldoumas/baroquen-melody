using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.FourFour;

internal sealed class TurnInvertedTurnOrnamentationCleaner(CompositionConfiguration compositionConfiguration) : IProcessor<OrnamentationCleaningItem>
{
    private const int IndexToCheck = 1;

    public void Process(OrnamentationCleaningItem item)
    {
        if (item.Note.OrnamentationType is OrnamentationType.Turn)
        {
            Clean(item.Note, item.OtherNote);
        }
        else
        {
            Clean(item.OtherNote, item.Note);
        }
    }

    private void Clean(BaroquenNote noteWithTurnOrnamentation, BaroquenNote noteWithInvertedTurnOrnamentation)
    {
        if (!noteWithTurnOrnamentation.Ornamentations[IndexToCheck].IsDissonantWith(noteWithInvertedTurnOrnamentation.Ornamentations[IndexToCheck]))
        {
            return;
        }

        noteWithInvertedTurnOrnamentation.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
    }
}
