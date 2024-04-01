using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.Interaction;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Compositions.Domain;

/// <summary>
///    Represents a note in a composition.
/// </summary>
/// <param name="voice">The voice that the note is played in.</param>
/// <param name="raw">The raw note that is played.</param>
internal sealed class BaroquenNote(Voice voice, Note raw)
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
    public bool HasOrnamentations => Ornamentations.Any();
}
