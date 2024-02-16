using BaroquenMelody.Library.Compositions;
using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Contexts;
using Melanchall.DryWetMidi.MusicTheory;
using Chord = BaroquenMelody.Library.Compositions.Chord;

namespace BaroquenMelody.Library.Extensions;

internal static class ChordContextExtensions
{
    /// <summary>
    ///     Applies the given <see cref="ChordChoice"/> to the given <see cref="ChordContext"/> to generate the next chord.
    /// </summary>
    /// <param name="chordContext"> The chord context. </param>
    /// <param name="chordChoice"> The chord choice. </param>
    /// <param name="scale"> The scale to be used in the generation of the next chord. </param>
    /// <returns> The next chord. </returns>
    public static Chord ApplyChordChoice(this ChordContext chordContext, ChordChoice chordChoice, Scale scale)
    {
        var notes = new HashSet<VoicedNote>();

        foreach (var noteChoice in chordChoice.NoteChoices)
        {
            var noteContext = chordContext[noteChoice.Voice];

            notes.Add(noteContext.ApplyNoteChoice(noteChoice, scale));
        }

        return new Chord(
            notes,
            chordContext,
            chordChoice
        );
    }
}
