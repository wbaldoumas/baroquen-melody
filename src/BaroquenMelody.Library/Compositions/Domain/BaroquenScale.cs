﻿using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Compositions.Domain;

/// <summary>
///    Represents a scale in a musical composition.
/// </summary>
public sealed class BaroquenScale
{
    private readonly List<Note> _notes;

    private readonly FrozenDictionary<SevenBitNumber, List<Note>> _ascendingNotesByNoteNumber;

    private readonly FrozenDictionary<SevenBitNumber, List<Note>> _descendingNotesByNoteNumber;

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

    /// <summary>
    ///     The mode of the scale.
    /// </summary>
    public Mode Mode { get; }

    public BaroquenScale(NoteName tonic, Mode mode)
        : this(Scale.Parse($"{tonic} {mode}"))
    {
        Mode = mode;
    }

    private BaroquenScale(Scale raw)
    {
        Raw = raw;
        _notes = raw.GetNotes().ToList();

        var ascendingNotesByNoteNumber = new Dictionary<SevenBitNumber, List<Note>>();
        var descendingNotesByNoteNumber = new Dictionary<SevenBitNumber, List<Note>>();

        foreach (var note in _notes)
        {
            ascendingNotesByNoteNumber[note.NoteNumber] = Raw.GetAscendingNotes(note).ToList();
            descendingNotesByNoteNumber[note.NoteNumber] = Raw.GetDescendingNotes(note).ToList();
        }

        _ascendingNotesByNoteNumber = ascendingNotesByNoteNumber.ToFrozenDictionary();
        _descendingNotesByNoteNumber = descendingNotesByNoteNumber.ToFrozenDictionary();

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
    public List<Note> GetAscendingNotes(Note note) => _ascendingNotesByNoteNumber[note.NoteNumber];

    /// <summary>
    ///     Retrieve the descending notes from the given note.
    /// </summary>
    /// <param name="note">The note to retrieve the descending notes from.</param>
    /// <returns>The descending notes from the given note.</returns>
    public List<Note> GetDescendingNotes(Note note) => _descendingNotesByNoteNumber[note.NoteNumber];

    /// <summary>
    ///     Retrieve the index of the given note in the scale.
    /// </summary>
    /// <param name="note">The note to retrieve the index of.</param>
    /// <returns>The index of the given note in the scale.</returns>
    public int IndexOf(BaroquenNote note) => GetNotes().IndexOf(note.Raw);
}
