using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Melanchall.DryWetMidi.Interaction;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Compositions.Domain;

/// <summary>
///    Represents a note in a composition.
/// </summary>
/// <param name="voice">The voice that the note is played in.</param>
/// <param name="raw">The raw note that is played.</param>
internal sealed class BaroquenNote(Voice voice, Note raw) : IEquatable<BaroquenNote>
{
    /// <summary>
    ///     The voice that the note is played in.
    /// </summary>
    public Voice Voice { get; init; } = voice;

    /// <summary>
    ///     The raw note that is played.
    /// </summary>
    public Note Raw { get; init; } = raw;

    /// <summary>
    ///     The duration of the note. May be modified if the note is ornamented.
    /// </summary>
    public MusicalTimeSpan Duration { get; set; } = MusicalTimeSpan.Quarter;

    /// <summary>
    ///     The ornamentation notes that are played with this note.
    /// </summary>
    public IList<BaroquenNote> Ornamentations { get; } = [];

    /// <summary>
    ///     Whether or not this note has any ornamentations.
    /// </summary>
    public bool HasOrnamentations => OrnamentationType != OrnamentationType.None;

    /// <summary>
    ///     The type of ornamentation that is applied to this note.
    /// </summary>
    public OrnamentationType OrnamentationType { get; set; } = OrnamentationType.None;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaroquenNote"/> class.
    /// </summary>
    /// <param name="note">The note to copy.</param>
    public BaroquenNote(BaroquenNote note)
        : this(note.Voice, note.Raw)
    {
        Duration = note.Duration;
        Ornamentations = note.Ornamentations.Select(ornamentation => new BaroquenNote(ornamentation)).ToList();
    }

    /// <summary>
    ///     Resets the ornamentation on this note.
    /// </summary>
    public void ResetOrnamentation()
    {
        Duration = MusicalTimeSpan.Quarter;
        Ornamentations.Clear();
        OrnamentationType = OrnamentationType.None;
    }

    /// <summary>
    ///     Determines if the <see cref="BaroquenNote"/> is equal to another <see cref="BaroquenNote"/>.
    /// </summary>
    /// <param name="other">The other <see cref="BaroquenNote"/> to compare against.</param>
    /// <returns>Whether the <see cref="BaroquenNote"/> is equal to the other <see cref="BaroquenNote"/>.</returns>
    public bool Equals(BaroquenNote? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Voice == other.Voice &&
               Raw == other.Raw &&
               Duration == other.Duration &&
               Ornamentations.SequenceEqual(other.Ornamentations);
    }

    /// <summary>
    ///     Determines if the <see cref="BaroquenNote"/> is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare against.</param>
    /// <returns>Whether the <see cref="BaroquenNote"/> is equal to the other object.</returns>
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is BaroquenNote other && Equals(other));

    /// <summary>
    ///    Throws an <see cref="InvalidOperationException"/> since <see cref="BaroquenNote"/> cannot be used as a hash key.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
#pragma warning disable SA1615
    public override int GetHashCode() => throw new InvalidOperationException($"{nameof(BaroquenNote)} cannot be used as a hash key since it has mutable properties.");
#pragma warning restore SA1615

    /// <summary>
    ///     Determines if the <see cref="BaroquenNote"/> is equal to another <see cref="BaroquenNote"/>.
    /// </summary>
    /// <param name="note">The first <see cref="BaroquenNote"/> to compare.</param>
    /// <param name="otherNote">The second <see cref="BaroquenNote"/> to compare.</param>
    /// <returns>Whether the <see cref="BaroquenNote"/> is equal to the other <see cref="BaroquenNote"/>.</returns>
    public static bool operator ==(BaroquenNote? note, BaroquenNote? otherNote)
    {
        if (ReferenceEquals(note, otherNote))
        {
            return true;
        }

        if (note is null || otherNote is null)
        {
            return false;
        }

        return note.Equals(otherNote);
    }

    /// <summary>
    ///     Determines if the <see cref="BaroquenNote"/> is not equal to another <see cref="BaroquenNote"/>.
    /// </summary>
    /// <param name="note">The first <see cref="BaroquenNote"/> to compare.</param>
    /// <param name="otherNote">The second <see cref="BaroquenNote"/> to compare.</param>
    /// <returns>Whether the <see cref="BaroquenNote"/> is not equal to the other <see cref="BaroquenNote"/>.</returns>
    public static bool operator !=(BaroquenNote? note, BaroquenNote? otherNote) => !(note == otherNote);
}
