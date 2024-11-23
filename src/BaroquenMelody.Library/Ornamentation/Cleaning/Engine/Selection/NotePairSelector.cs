using BaroquenMelody.Library.Ornamentation.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection;

internal sealed class NotePairSelector(
    IEnumerable<OrnamentationType> primaryNoteOrnamentationTypes,
    IEnumerable<OrnamentationType> secondaryNoteOrnamentationTypes
) : INotePairSelector
{
    private readonly FrozenSet<OrnamentationType> _primaryNoteOrnamentationTypes = primaryNoteOrnamentationTypes.ToFrozenSet();

    private readonly FrozenSet<OrnamentationType> _secondaryNoteOrnamentationTypes = secondaryNoteOrnamentationTypes.ToFrozenSet();

    public NotePairSelector(OrnamentationType primaryNoteOrnamentationType, OrnamentationType secondaryNoteOrnamentationType)
        : this([primaryNoteOrnamentationType], [secondaryNoteOrnamentationType])
    {
    }

    public NotePair Select(OrnamentationCleaningItem item)
    {
        if (_primaryNoteOrnamentationTypes.Contains(item.Note.OrnamentationType) && _secondaryNoteOrnamentationTypes.Contains(item.OtherNote.OrnamentationType))
        {
            return new NotePair(item.Note, item.OtherNote);
        }

        return _primaryNoteOrnamentationTypes.Contains(item.OtherNote.OrnamentationType) && _secondaryNoteOrnamentationTypes.Contains(item.Note.OrnamentationType)
            ? new NotePair(item.OtherNote, item.Note)
            : throw new InvalidOperationException("The ornamentation cleaning item does not contain the expected ornamentation types.");
    }
}
