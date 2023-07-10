namespace BaroquenMelody.Library.Composition.Choices;

/// <summary>
///     Represents the note choices for the voices in a given chord to arrive at the next chord.
/// </summary>
/// <param name="NoteChoices"> The note choices for the voices in a given chord to arrive at the next chord. </param>
internal sealed record ChordChoice(ISet<NoteChoice> NoteChoices)
{
    public bool Equals(ChordChoice? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || NoteChoices.SetEquals(other.NoteChoices);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return NoteChoices.Aggregate(1430287, (current, noteChoice) => current * 7302013 ^ noteChoice.GetHashCode());
        }
    }
}
