using BaroquenMelody.Library.Compositions;
using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Extensions;

internal static class NoteContextExtensions
{
    /// <summary>
    ///     Applies the given <see cref="NoteChoice"/> to the given <see cref="NoteContext"/> to generate the next note.
    /// </summary>
    /// <param name="noteContext"> The note context. </param>
    /// <param name="noteChoice"> The note choice. </param>
    /// <param name="scale"> The scale to be used in the generation of the next note. </param>
    /// <returns> The next note. </returns>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown when the given <see cref="NoteChoice"/> has an invalid <see cref="NoteMotion"/>. </exception>
    public static VoicedNote ApplyNoteChoice(this NoteContext noteContext, NoteChoice noteChoice, Scale scale)
    {
        var notes = scale.GetDescendingNotes(noteContext.Note);
        var ascendingNotes = scale.GetAscendingNotes(noteContext.Note);

        var note = noteChoice.Motion switch
        {
            NoteMotion.Ascending => scale.GetAscendingNotes(noteContext.Note).ElementAt(noteChoice.ScaleStepChange),
            NoteMotion.Descending => scale.GetDescendingNotes(noteContext.Note).ElementAt(noteChoice.ScaleStepChange),
            NoteMotion.Oblique => noteContext.Note,
            _ => throw new ArgumentOutOfRangeException(nameof(noteChoice))
        };

        return new VoicedNote(
            note,
            noteChoice.Voice,
            noteContext,
            noteChoice
        );
    }
}
