using BaroquenMelody.Library.Compositions.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Compositions.Domain;

/// <summary>
///    Represents a chord in a composition.
/// </summary>
/// <param name="notes">The notes that are played during the chord.</param>
internal sealed class BaroquenChord(IEnumerable<BaroquenNote> notes) : IEquatable<BaroquenChord>
{
    public IEnumerable<BaroquenNote> Notes => _notesByVoice.Values;

    public BaroquenNote this[Voice voice] => _notesByVoice[voice];

    private readonly FrozenDictionary<Voice, BaroquenNote> _notesByVoice = notes.ToFrozenDictionary(note => note.Voice);

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaroquenChord"/> class.
    /// </summary>
    /// <param name="chord">The chord to copy.</param>
    public BaroquenChord(BaroquenChord chord)
        : this(chord.Notes.Select(note => new BaroquenNote(note)))
    {
    }

    /// <summary>
    ///     Determines if the <see cref="BaroquenChord"/> is equal to another <see cref="BaroquenChord"/>.
    /// </summary>
    /// <param name="other">The other <see cref="BaroquenChord"/> to compare against.</param>
    /// <returns>Whether the <see cref="BaroquenChord"/> is equal to the other <see cref="BaroquenChord"/>.</returns>
    public bool Equals(BaroquenChord? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || Notes.SequenceEqual(other.Notes);
    }

    /// <summary>
    ///     Determines if the <see cref="BaroquenChord"/> is equal to another object.
    /// </summary>
    /// <param name="obj">The other object to compare against.</param>
    /// <returns>Whether the <see cref="BaroquenChord"/> is equal to the other object.</returns>
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is BaroquenChord other && Equals(other));

    /// <summary>
    ///    Throws an <see cref="InvalidOperationException"/> since <see cref="BaroquenChord"/> cannot be used as a hash key.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
#pragma warning disable SA1615
    public override int GetHashCode() => throw new InvalidOperationException($"{nameof(BaroquenChord)} cannot be used as a hash key since it has mutable properties.");
#pragma warning restore SA1615

    /// <summary>
    ///     Determines if the <see cref="BaroquenChord"/> is equal to another <see cref="BaroquenChord"/>.
    /// </summary>
    /// <param name="chord">The first <see cref="BaroquenChord"/> to compare.</param>
    /// <param name="otherChord">The second <see cref="BaroquenChord"/> to compare.</param>
    /// <returns>Whether the <see cref="BaroquenChord"/> is equal to the other <see cref="BaroquenChord"/>.</returns>
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

    /// <summary>
    ///     Determines if the <see cref="BaroquenChord"/> is not equal to another <see cref="BaroquenChord"/>.
    /// </summary>
    /// <param name="chord">The first <see cref="BaroquenChord"/> to compare.</param>
    /// <param name="otherChord">The second <see cref="BaroquenChord"/> to compare.</param>
    /// <returns>Whether the <see cref="BaroquenChord"/> is not equal to the other <see cref="BaroquenChord"/>.</returns>
    public static bool operator !=(BaroquenChord? chord, BaroquenChord? otherChord) => !(chord == otherChord);
}
