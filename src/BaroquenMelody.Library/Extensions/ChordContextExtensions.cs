using BaroquenMelody.Library.Composition;
using BaroquenMelody.Library.Composition.Choices;
using BaroquenMelody.Library.Composition.Contexts;

namespace BaroquenMelody.Library.Extensions;

internal static class ChordContextExtensions
{
    /// <summary>
    ///     Applies the given <see cref="ChordChoice"/> to the given <see cref="ChordContext"/> to generate the next chord.
    /// </summary>
    /// <param name="chordContext"> The chord context. </param>
    /// <param name="chordChoice"> The chord choice. </param>
    /// <returns> The next chord. </returns>
    public static Chord ApplyChordChoice(this ChordContext chordContext, ChordChoice chordChoice)
    {
        var notes = new HashSet<Note>();

        foreach (var noteChoice in chordChoice.NoteChoices)
        {
            var noteContext = chordContext[noteChoice.Voice];

            notes.Add(noteContext.ApplyNoteChoice(noteChoice));
        }

        return new Chord(
            notes,
            chordContext,
            chordChoice
        );
    }
}
