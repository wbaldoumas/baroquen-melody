using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Domain;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Contexts.Extensions;

internal static class ChordContextExtensions
{
    /// <summary>
    ///     Applies the given <see cref="ChordChoice"/> to the given <see cref="ChordContext"/> to generate the next chord.
    /// </summary>
    /// <param name="chordContext"> The chord context. </param>
    /// <param name="chordChoice"> The chord choice. </param>
    /// <param name="scale"> The scale to be used in the generation of the next chord. </param>
    /// <returns> The next chord. </returns>
    public static ContextualizedChord ApplyChordChoice(this ChordContext chordContext, ChordChoice chordChoice, Scale scale)
    {
        var notes = new HashSet<ContextualizedNote>();

        foreach (var noteChoice in chordChoice.NoteChoices)
        {
            var noteContext = chordContext[noteChoice.Voice];

            notes.Add(noteContext.ApplyNoteChoice(noteChoice, scale));
        }

        return new ContextualizedChord(
            notes,
            chordContext,
            chordChoice
        );
    }
}
