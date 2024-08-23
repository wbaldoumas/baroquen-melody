using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Domain;

/// <summary>
///    Represents a scale in a musical composition.
/// </summary>
public sealed class BaroquenScale
{
    private readonly List<Note> _notes;

    private readonly IDictionary<Note, List<Note>> _ascendingNotes = new Dictionary<Note, List<Note>>();

    private readonly IDictionary<Note, List<Note>> _descendingNotes = new Dictionary<Note, List<Note>>();

    /// <summary>
    ///     The tonic note of the scale (1st scale degree).
    /// </summary>
    public NoteName Tonic { get; }

    /// <summary>
    ///     The supertonic note of the scale (2nd scale degree).
    /// </summary>
    public NoteName Supertonic { get; }

    /// <summary>
    ///     The mediant note of the scale (3rd scale degree).
    /// </summary>
    public NoteName Mediant { get; }

    /// <summary>
    ///     The subdominant note of the scale (4th scale degree).
    /// </summary>
    public NoteName Subdominant { get; }

    /// <summary>
    ///     The dominant note of the scale (5th scale degree).
    /// </summary>
    public NoteName Dominant { get; }

    /// <summary>
    ///     The submediant note of the scale (6th scale degree).
    /// </summary>
    public NoteName Submediant { get; }

    /// <summary>
    ///     The leading tone of the scale (7th scale degree).
    /// </summary>
    public NoteName LeadingTone { get; }

    /// <summary>
    ///     The one chord of the scale.
    /// </summary>
    public HashSet<NoteName> I { get; }

    /// <summary>
    ///     The two chord of the scale.
    /// </summary>
    public HashSet<NoteName> II { get; }

    /// <summary>
    ///     The three chord of the scale.
    /// </summary>
    public HashSet<NoteName> III { get; }

    /// <summary>
    ///     The four chord of the scale.
    /// </summary>
    public HashSet<NoteName> IV { get; }

    /// <summary>
    ///     The five chord of the scale.
    /// </summary>
    public HashSet<NoteName> V { get; }

    /// <summary>
    ///     The six chord of the scale.
    /// </summary>
    public HashSet<NoteName> VI { get; }

    /// <summary>
    ///     The seven chord of the scale.
    /// </summary>
    public HashSet<NoteName> VII { get; }

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

        Tonic = raw.GetDegree(ScaleDegree.Tonic);
        Supertonic = raw.GetDegree(ScaleDegree.Supertonic);
        Mediant = raw.GetDegree(ScaleDegree.Mediant);
        Subdominant = raw.GetDegree(ScaleDegree.Subdominant);
        Dominant = raw.GetDegree(ScaleDegree.Dominant);
        Submediant = raw.GetDegree(ScaleDegree.Submediant);
        LeadingTone = raw.GetDegree(ScaleDegree.LeadingTone);

        I = [Tonic, Mediant, Dominant];
        II = [Supertonic, Subdominant, Submediant];
        III = [Mediant, Dominant, LeadingTone];
        IV = [Subdominant, Submediant, Tonic];
        V = [Dominant, LeadingTone, Supertonic];
        VI = [Submediant, Tonic, Mediant];
        VII = [LeadingTone, Supertonic, Subdominant];
    }

    public BaroquenScale(NoteName tonic, Mode mode)
        : this(Scale.Parse($"{tonic} {mode}"))
    {
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
    public List<Note> GetNotes() => _notes;

    /// <summary>
    ///     Retrieve the ascending notes from the given note.
    /// </summary>
    /// <param name="note">The note to retrieve the ascending notes from.</param>
    /// <returns>The ascending notes from the given note.</returns>
    public List<Note> GetAscendingNotes(Note note) => _ascendingNotes[note];

    /// <summary>
    ///     Retrieve the descending notes from the given note.
    /// </summary>
    /// <param name="note">The note to retrieve the descending notes from.</param>
    /// <returns>The descending notes from the given note.</returns>
    public List<Note> GetDescendingNotes(Note note) => _descendingNotes[note];

    /// <summary>
    ///     Retrieve the index of the given note in the scale.
    /// </summary>
    /// <param name="note">The note to retrieve the index of.</param>
    /// <returns>The index of the given note in the scale.</returns>
    public int IndexOf(BaroquenNote note) => GetNotes().IndexOf(note.Raw);
}
