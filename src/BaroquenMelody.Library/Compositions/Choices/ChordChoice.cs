using BaroquenMelody.Library.Infrastructure.Equality;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <summary>
///     Represents the note choices for the voices in a given chord to move to the next chord.
/// </summary>
internal sealed record ChordChoice
{
    private readonly IList<NoteChoice> _noteChoices;

    public ChordChoice(IEnumerable<NoteChoice> noteChoices) =>
        _noteChoices = noteChoices.OrderBy(noteChoice => noteChoice.Voice).ToList();

    public IList<NoteChoice> NoteChoices
    {
        get => _noteChoices;
        init { _noteChoices = value.OrderBy(noteChoice => noteChoice.Voice).ToList(); }
    }

    public bool Equals(ChordChoice? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || NoteChoices.SequenceEqual(other.NoteChoices);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return NoteChoices.Aggregate(
                HashCodeGeneration.InitialValue,
                (current, noteChoice) => current * HashCodeGeneration.Multiplier ^ noteChoice.GetHashCode());
        }
    }
}
