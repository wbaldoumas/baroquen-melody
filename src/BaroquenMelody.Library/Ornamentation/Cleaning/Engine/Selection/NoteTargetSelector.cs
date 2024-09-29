using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection.Strategies;

namespace BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection;

internal sealed class NoteTargetSelector(IReadOnlyCollection<IOrnamentationCleaningSelectorStrategy> cleaningSelectorStrategies) : INoteTargetSelector
{
    public BaroquenNote Select(OrnamentationCleaningItem item)
    {
        foreach (var strategy in cleaningSelectorStrategies)
        {
            var selectedNote = strategy.Select(item.Note, item.OtherNote);

            if (selectedNote is not null)
            {
                return selectedNote;
            }

            selectedNote = strategy.Select(item.OtherNote, item.Note);

            if (selectedNote is not null)
            {
                return selectedNote;
            }
        }

        throw new InvalidOperationException("No strategy selected a note to clean.");
    }
}
