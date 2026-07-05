using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using Melanchall.DryWetMidi.Interaction;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Extensions;

/// <summary>
///     A home for extension methods for <see cref="BaroquenNote"/>.
/// </summary>
internal static class BaroquenNoteExtensions
{
    public static BaroquenNote ApplyNoteChoice(this BaroquenNote note, BaroquenScale scale, NoteChoice noteChoice, MusicalTimeSpan noteTimeSpan) =>
        new(note.Instrument, note.GetTargetRawNote(scale, noteChoice), noteTimeSpan);

    /// <summary>
    ///     Resolves the raw note the given note choice would move this note to, without materializing a new
    ///     <see cref="BaroquenNote"/>.
    /// </summary>
    /// <param name="note">The note the choice is applied to.</param>
    /// <param name="scale">The scale to move within.</param>
    /// <param name="noteChoice">The motion and scale-step change to apply.</param>
    /// <returns>The raw note the choice lands on.</returns>
    public static Note GetTargetRawNote(this BaroquenNote note, BaroquenScale scale, NoteChoice noteChoice) => noteChoice.Motion switch
    {
        NoteMotion.Ascending => scale.GetAscendingNotes(note.Raw)[noteChoice.ScaleStepChange],
        NoteMotion.Descending => scale.GetDescendingNotes(note.Raw)[noteChoice.ScaleStepChange],
        NoteMotion.Oblique => note.Raw,
        _ => throw new ArgumentOutOfRangeException(nameof(noteChoice))
    };

    public static bool IsDissonantWith(this BaroquenNote note, BaroquenNote otherNote) => note.Raw.IsDissonantWith(otherNote.Raw);
}
