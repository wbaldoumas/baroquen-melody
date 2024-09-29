using BaroquenMelody.Library.Domain;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.MusicTheory.Enums.Extensions;

/// <summary>
///     A home for extensions and utilities for the <see cref="Interval"/> enum.
/// </summary>
internal static class IntervalExtensions
{
    private const int Octave = 12;

    /// <summary>
    ///     Generate an <see cref="Interval"/> from two <see cref="BaroquenNote"/>s.
    /// </summary>
    /// <param name="note">The first note.</param>
    /// <param name="otherNote">The second note.</param>
    /// <returns>An <see cref="Interval"/> representing the distance between the two notes.</returns>
    public static Interval FromNotes(BaroquenNote note, BaroquenNote otherNote) => FromNotes(note.Raw, otherNote.Raw);

    /// <summary>
    ///     Generate an <see cref="Interval"/> from two <see cref="Note"/>s.
    /// </summary>
    /// <param name="note">The first note.</param>
    /// <param name="otherNote">The second note.</param>
    /// <returns>An <see cref="Interval"/> representing the distance between the two notes.</returns>
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
    public static Interval FromNotes(Note note, Note otherNote) => Math.Abs(note.NoteNumber % Octave - otherNote.NoteNumber % Octave) switch
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
    {
        0 => Interval.Unison,
        1 => Interval.MinorSecond,
        2 => Interval.MajorSecond,
        3 => Interval.MinorThird,
        4 => Interval.MajorThird,
        5 => Interval.PerfectFourth,
        6 => Interval.Tritone,
        7 => Interval.PerfectFifth,
        8 => Interval.MinorSixth,
        9 => Interval.MajorSixth,
        10 => Interval.MinorSeventh,
        11 => Interval.MajorSeventh
    };
}
