using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Contexts;

/// <inheritdoc cref="IChordContextGenerator"/>
internal sealed class ChordContextGenerator : IChordContextGenerator
{
    private const int LeapThreshold = 4;

    public ChordContext GenerateChordContext(ContextualizedChord previousChord, ContextualizedChord currentChord)
    {
        var noteContexts = previousChord.Notes
            .Select(contextualizedNote => contextualizedNote.Voice)
            .Select(voice =>
            {
                var previousNote = previousChord.Notes.First(note => note.Voice == voice).Note;
                var currentNote = currentChord.Notes.First(note => note.Voice == voice).Note;

                var noteMotion = previousNote.CompareTo(currentNote) switch
                {
                    < 0 => NoteMotion.Ascending,
                    > 0 => NoteMotion.Descending,
                    _ => NoteMotion.Oblique
                };

                var noteSpan = Math.Abs(previousNote.NoteNumber - currentNote.NoteNumber) switch
                {
                    < 0 => throw new InvalidOperationException("The note span should not be negative."),
                    0 => NoteSpan.None,
                    > 0 and <= LeapThreshold => NoteSpan.Step,
                    > LeapThreshold => NoteSpan.Leap
                };

                return new NoteContext(voice, currentNote, noteMotion, noteSpan);
            });

        return new ChordContext(noteContexts);
    }
}
