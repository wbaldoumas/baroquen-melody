using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;
using Note = BaroquenMelody.Library.Compositions.Domain.Note;

namespace BaroquenMelody.Library.Compositions.Extensions;

/// <summary>
///     A home for extension methods for <see cref="Domain.Note"/>.
/// </summary>
internal static class BaroquenNoteExtensions
{
    public static Note ApplyNoteChoice(this Note note, Scale scale, NoteChoice noteChoice)
    {
        var nextNote = noteChoice.Motion switch
        {
            NoteMotion.Ascending => scale.GetAscendingNotes(note.Raw).ElementAt(noteChoice.ScaleStepChange),
            NoteMotion.Descending => scale.GetDescendingNotes(note.Raw).ElementAt(noteChoice.ScaleStepChange),
            NoteMotion.Oblique => note.Raw,
            _ => throw new ArgumentOutOfRangeException(nameof(noteChoice))
        };

        return note with { Raw = nextNote };
    }
}
