using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Extensions;

/// <summary>
///     A home for extension methods for <see cref="BaroquenNote"/>.
/// </summary>
internal static class BaroquenNoteExtensions
{
    public static BaroquenNote ApplyNoteChoice(this BaroquenNote note, BaroquenScale scale, NoteChoice noteChoice, MusicalTimeSpan noteTimeSpan)
    {
        var nextNote = noteChoice.Motion switch
        {
            NoteMotion.Ascending => scale.GetAscendingNotes(note.Raw)[noteChoice.ScaleStepChange],
            NoteMotion.Descending => scale.GetDescendingNotes(note.Raw)[noteChoice.ScaleStepChange],
            NoteMotion.Oblique => note.Raw,
            _ => throw new ArgumentOutOfRangeException(nameof(noteChoice))
        };

        return new BaroquenNote(note.Instrument, nextNote, noteTimeSpan);
    }

    public static bool IsDissonantWith(this BaroquenNote note, BaroquenNote otherNote) => note.Raw.IsDissonantWith(otherNote.Raw);
}
