using BaroquenMelody.Library.Compositions.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Compositions.Domain;

/// <summary>
///    Represents a chord in a composition.
/// </summary>
/// <param name="notes">The notes that are played during the chord.</param>
internal sealed class BaroquenChord(IEnumerable<BaroquenNote> notes) : IEquatable<BaroquenChord>
{
    public IEnumerable<BaroquenNote> Notes => _notes.Values;

    public BaroquenNote this[Voice voice] => _notes[voice];

    private readonly FrozenDictionary<Voice, BaroquenNote> _notes = notes.ToFrozenDictionary(note => note.Voice);

    public bool Equals(BaroquenChord? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || Notes.SequenceEqual(other.Notes);
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is BaroquenChord other && Equals(other));

    /// <summary>
    ///    Throws an <see cref="InvalidOperationException"/> since <see cref="BaroquenChord"/> cannot be used as a hash key.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
#pragma warning disable SA1615
    public override int GetHashCode() => throw new InvalidOperationException($"{nameof(BaroquenChord)} cannot be used as a hash key since it has mutable properties.");
#pragma warning restore SA1615

    public static bool operator ==(BaroquenChord? chord, BaroquenChord? otherChord)
    {
        if (ReferenceEquals(chord, otherChord))
        {
            return true;
        }

        if (chord is null || otherChord is null)
        {
            return false;
        }

        return chord.Equals(otherChord);
    }

    public static bool operator !=(BaroquenChord? chord, BaroquenChord? otherChord) => !(chord == otherChord);
}
