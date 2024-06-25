using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Domain;

/// <summary>
///    Represents a scale in a musical composition.
/// </summary>
internal sealed class BaroquenScale
{
    private readonly IReadOnlyList<Note> _notes;

    private readonly IDictionary<Note, IReadOnlyList<Note>> _ascendingNotes = new Dictionary<Note, IReadOnlyList<Note>>();

    private readonly IDictionary<Note, IReadOnlyList<Note>> _descendingNotes = new Dictionary<Note, IReadOnlyList<Note>>();

    /// <summary>
    ///     The raw scale that this Baroquen scale is based on.
    /// </summary>
    public Scale Raw { get; }

    public BaroquenScale(Scale raw)
    {
        Raw = raw;
        _notes = raw.GetNotes().ToList();

        foreach (var note in _notes)
        {
            _ascendingNotes[note] = Raw.GetAscendingNotes(note).ToList();
            _descendingNotes[note] = Raw.GetDescendingNotes(note).ToList();
        }
    }

    /// <summary>
    ///     Converts a string representation of a musical scale into its equivalent <see cref="BaroquenScale"/>.
    /// </summary>
    /// <param name="scale">The string representation of a musical scale.</param>
    /// <returns>The equivalent <see cref="BaroquenScale"/>.</returns>
    public static BaroquenScale Parse(string scale) => new(Scale.Parse(scale));

    /// <summary>
    ///     Retrieve all notes in the scale.
    /// </summary>
    /// <returns>All notes in the scale.</returns>
    public IReadOnlyList<Note> GetNotes() => _notes;

    /// <summary>
    ///     Retrieve the ascending notes from the given note.
    /// </summary>
    /// <param name="note">The note to retrieve the ascending notes from.</param>
    /// <returns>The ascending notes from the given note.</returns>
    public IReadOnlyList<Note> GetAscendingNotes(Note note) => _ascendingNotes[note];

    /// <summary>
    ///     Retrieve the descending notes from the given note.
    /// </summary>
    /// <param name="note">The note to retrieve the descending notes from.</param>
    /// <returns>The descending notes from the given note.</returns>
    public IReadOnlyList<Note> GetDescendingNotes(Note note) => _descendingNotes[note];
}
