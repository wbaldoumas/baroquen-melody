using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Domain;

/// <summary>
///    Represents a note in a composition.
/// </summary>
/// <param name="instrument">The instrument that the note is played by.</param>
/// <param name="raw">The raw note that is played.</param>
/// <param name="musicalTimeSpan">The musical time span of the note. May be modified if the note is ornamented.</param>
public sealed class BaroquenNote(Instrument instrument, Note raw, MusicalTimeSpan musicalTimeSpan) : IEquatable<BaroquenNote>
{
    /// <summary>
    ///     The instrument that the note is played by.
    /// </summary>
    public Instrument Instrument { get; } = instrument;

    /// <summary>
    ///     The raw note that is played.
    /// </summary>
    public Note Raw { get; } = raw;

    /// <summary>
    ///     The musical time span of the note. May be modified if the note is ornamented.
    /// </summary>
    public MusicalTimeSpan MusicalTimeSpan { get; set; } = musicalTimeSpan;

    /// <summary>
    ///     The name of the note.
    /// </summary>
    public NoteName NoteName => Raw.NoteName;

    /// <summary>
    ///     The number of the note.
    /// </summary>
    public SevenBitNumber NoteNumber => Raw.NoteNumber;

    /// <summary>
    ///     The ornamentation notes that are played with this note.
    /// </summary>
    public IList<BaroquenNote> Ornamentations { get; } = [];

    /// <summary>
    ///     Whether or not this note has any ornamentations.
    /// </summary>
    public bool HasOrnamentations => Ornamentations.Count > 0 || OrnamentationType != OrnamentationType.None;

    /// <summary>
    ///     The type of ornamentation that is applied to this note.
    /// </summary>
    public OrnamentationType OrnamentationType { get; set; } = OrnamentationType.None;

    /// <summary>
    ///     The velocity of the note, impacting dynamics.
    /// </summary>
    public SevenBitNumber Velocity { get; set; } = new(75);

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaroquenNote"/> class.
    /// </summary>
    /// <param name="note">The note to copy.</param>
    public BaroquenNote(BaroquenNote note)
        : this(note.Instrument, note.Raw, note.MusicalTimeSpan)
    {
        Ornamentations = note.Ornamentations.Select(ornamentation => new BaroquenNote(ornamentation)).ToList();
        OrnamentationType = note.OrnamentationType;
    }

    /// <summary>
    ///     Resets the ornamentation of the <see cref="BaroquenNote"/>.
    /// </summary>
    /// <param name="defaultTimeSpan">The time span to reset the note to.</param>
    public void ResetOrnamentation(MusicalTimeSpan defaultTimeSpan)
    {
        MusicalTimeSpan = defaultTimeSpan;
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

        return Instrument == other.Instrument &&
               Raw == other.Raw &&
               MusicalTimeSpan == other.MusicalTimeSpan &&
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
    /// <returns>Always throws an <see cref="InvalidOperationException"/>.</returns>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    public override int GetHashCode() => throw new InvalidOperationException($"{nameof(BaroquenNote)} cannot be used as a hash key since it has mutable properties.");

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

    /// <summary>
    ///     Determines if the <see cref="BaroquenNote"/> is greater (higher in pitch) than another <see cref="BaroquenNote"/>.
    /// </summary>
    /// <param name="note">The first <see cref="BaroquenNote"/> to compare.</param>
    /// <param name="otherNote">The second <see cref="BaroquenNote"/> to compare.</param>
    /// <returns>Whether the <see cref="BaroquenNote"/> is greater (higher in pitch) than the other <see cref="BaroquenNote"/>.</returns>
    public static bool operator >(BaroquenNote? note, BaroquenNote? otherNote) => note?.Raw.NoteNumber > otherNote?.Raw.NoteNumber;

    /// <summary>
    ///     Determines if the <see cref="BaroquenNote"/> is less (lower in pitch) than another <see cref="BaroquenNote"/>.
    /// </summary>
    /// <param name="note">The first <see cref="BaroquenNote"/> to compare.</param>
    /// <param name="otherNote">The second <see cref="BaroquenNote"/> to compare.</param>
    /// <returns>Whether the <see cref="BaroquenNote"/> is less (lower in pitch) than the other <see cref="BaroquenNote"/>.</returns>
    public static bool operator <(BaroquenNote? note, BaroquenNote? otherNote) => note?.Raw.NoteNumber < otherNote?.Raw.NoteNumber;
}
