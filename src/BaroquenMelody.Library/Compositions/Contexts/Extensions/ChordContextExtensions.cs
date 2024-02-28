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

    /// <summary>
    ///     Converts the given <see cref="ChordContext"/> to a <see cref="ContextualizedChord"/>.
    /// </summary>
    /// <remarks>
    ///     Note that the <see cref="ChordChoice"/> in the generated <see cref="ContextualizedChord"/> will be empty
    ///     since there is no information about which <see cref="ChordChoice"/> led to the given <see cref="ChordContext"/>.
    /// </remarks>
    /// <param name="chordContext"> The chord context to convert. </param>
    /// <returns> The converted contextualized chord. </returns>
    public static ContextualizedChord ToContextualizedChord(this ChordContext chordContext) => new(
        chordContext.NoteContexts.Select(noteContext =>
            new ContextualizedNote(
                noteContext.Note,
                noteContext.Voice,
                noteContext,
                NoteChoice.Empty // no information about the note choice to arrive at the note
            )
        ).ToHashSet(),
        chordContext,
        ChordChoice.Empty // no information about the chord choice to arrive at the chord
    );
}
