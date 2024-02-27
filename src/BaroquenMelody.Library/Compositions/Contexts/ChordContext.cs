using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Infrastructure.Equality;

namespace BaroquenMelody.Library.Compositions.Contexts;

/// <summary>
///     Represents the note contexts for the voices in a given chord used to arrive at the current chord.
/// </summary>
internal sealed record ChordContext
{
    private readonly IList<NoteContext> _noteContexts;

    public ChordContext(IEnumerable<NoteContext> noteContexts) =>
        _noteContexts = noteContexts.OrderBy(noteContext => noteContext.Voice).ToList();

    public IList<NoteContext> NoteContexts
    {
        get => _noteContexts;
        init { _noteContexts = value.OrderBy(noteContext => noteContext.Voice).ToList(); }
    }

    public NoteContext this[Voice voice] => NoteContexts.Single(noteContext => noteContext.Voice == voice);

    public bool Equals(ChordContext? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || NoteContexts.SequenceEqual(other.NoteContexts);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return NoteContexts.Aggregate(
                HashCodeGeneration.InitialValue,
                (current, noteChoice) => current * HashCodeGeneration.Multiplier ^ noteChoice.GetHashCode()
            );
        }
    }
}
