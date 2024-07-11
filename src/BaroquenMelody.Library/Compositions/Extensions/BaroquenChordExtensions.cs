using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Extensions;

/// <summary>
///     A home for extension methods for <see cref="BaroquenChord"/>.
/// </summary>
internal static class BaroquenChordExtensions
{
    public static BaroquenChord ApplyChordChoice(this BaroquenChord chord, BaroquenScale scale, ChordChoice chordChoice) => new(
        (
            from noteChoice in chordChoice.NoteChoices.Where(noteChoice => chord.Notes.Exists(note => note.Voice == noteChoice.Voice))
            let voice = noteChoice.Voice
            let note = chord[voice]
            select note.ApplyNoteChoice(scale, noteChoice)
        ).ToList()
    );
}
