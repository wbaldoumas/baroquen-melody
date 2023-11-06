using BaroquenMelody.Library.Compositions;
using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Extensions;

internal static class NoteContextExtensions
{
    /// <summary>
    ///     Applies the given <see cref="NoteChoice"/> to the given <see cref="NoteContext"/> to generate the next note.
    /// </summary>
    /// <param name="noteContext"> The note context. </param>
    /// <param name="noteChoice"> The note choice. </param>
    /// <returns> The next note. </returns>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown when the given <see cref="NoteChoice"/> has an invalid <see cref="NoteMotion"/>. </exception>
    public static Note ApplyNoteChoice(this NoteContext noteContext, NoteChoice noteChoice)
    {
        var pitch = noteChoice.Motion switch
        {
            NoteMotion.Ascending => noteContext.Pitch + noteChoice.PitchChange,
            NoteMotion.Descending => noteContext.Pitch - noteChoice.PitchChange,
            NoteMotion.Oblique => noteContext.Pitch,
            _ => throw new ArgumentOutOfRangeException(nameof(noteChoice))
        };

        return new Note(
            (byte)pitch,
            noteChoice.Voice,
            noteContext,
            noteChoice
        );
    }
}
