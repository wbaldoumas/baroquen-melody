using BaroquenMelody.Library.Domain;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Enums.Extensions;

/// <summary>
///     A home for extensions and utilities for the <see cref="NoteMotion"/> enum.
/// </summary>
internal static class NoteMotionExtensions
{
    /// <summary>
    ///     Generate a <see cref="NoteMotion"/> from two <see cref="Note"/>s.
    /// </summary>
    /// <param name="previousNote">The first note.</param>
    /// <param name="currentNote">The second note.</param>
    /// <returns>A <see cref="NoteMotion"/> representing the motion between the two notes.</returns>
    public static NoteMotion FromNotes(BaroquenNote previousNote, BaroquenNote currentNote) => FromNotes(previousNote.Raw, currentNote.Raw);

    /// <summary>
    ///     Generate a <see cref="NoteMotion"/> from two <see cref="Note"/>s.
    /// </summary>
    /// <param name="previousNote">The first note.</param>
    /// <param name="currentNote">The second note.</param>
    /// <returns>A <see cref="NoteMotion"/> representing the motion between the two notes.</returns>
    public static NoteMotion FromNotes(Note previousNote, Note currentNote)
    {
        if (previousNote == currentNote)
        {
            return NoteMotion.Oblique;
        }

        return previousNote.NoteNumber > currentNote.NoteNumber ? NoteMotion.Descending : NoteMotion.Ascending;
    }
}
