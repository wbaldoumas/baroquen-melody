namespace BaroquenMelody.Library.Composition.Contexts;

/// <summary>
///     Represents the note contexts for the voices in a given chord used to arrive at the current chord.
/// </summary>
/// <param name="NoteContexts"> The note contexts for the voices in a given chord used to arrive at the current chord. </param>
internal sealed record ChordContext(IList<NoteContext> NoteContexts)
{
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
                1430287,
                (current, noteChoice) => current * 7302013 ^ noteChoice.GetHashCode()
            );
        }
    }
}
