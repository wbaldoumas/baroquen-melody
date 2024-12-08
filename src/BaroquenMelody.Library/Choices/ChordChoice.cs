using BaroquenMelody.Infrastructure.Equality;

namespace BaroquenMelody.Library.Choices;

/// <summary>
///     Represents the note choices for the instruments in a given chord to move to the next chord.
/// </summary>
internal sealed record ChordChoice
{
    private readonly IList<NoteChoice> _noteChoices;

    public ChordChoice(IEnumerable<NoteChoice> noteChoices) =>
        _noteChoices = [.. noteChoices.OrderBy(static noteChoice => noteChoice.Instrument)];

    public IList<NoteChoice> NoteChoices
    {
        get => _noteChoices;
        init { _noteChoices = [.. value.OrderBy(static noteChoice => noteChoice.Instrument)]; }
    }

    public bool Equals(ChordChoice? other) => other is not null && (ReferenceEquals(this, other) || NoteChoices.SequenceEqual(other.NoteChoices));

    public override int GetHashCode()
    {
        unchecked
        {
            return NoteChoices.Aggregate(
                HashCodeGeneration.Seed,
                (current, noteChoice) => current * HashCodeGeneration.Multiplier ^ noteChoice.GetHashCode()
            );
        }
    }
}
