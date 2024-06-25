using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Extensions;

/// <summary>
///     A home for extension methods for <see cref="BaroquenNote"/>.
/// </summary>
internal static class BaroquenNoteExtensions
{
    public static BaroquenNote ApplyNoteChoice(this BaroquenNote note, BaroquenScale scale, NoteChoice noteChoice)
    {
        var nextNote = noteChoice.Motion switch
        {
            NoteMotion.Ascending => scale.GetAscendingNotes(note.Raw)[noteChoice.ScaleStepChange],
            NoteMotion.Descending => scale.GetDescendingNotes(note.Raw)[noteChoice.ScaleStepChange],
            NoteMotion.Oblique => note.Raw,
            _ => throw new ArgumentOutOfRangeException(nameof(noteChoice))
        };

        return new BaroquenNote(note.Voice, nextNote);
    }

    public static bool IsDissonantWith(this BaroquenNote note, BaroquenNote otherNote) => note.Raw.IsDissonantWith(otherNote.Raw);
}
