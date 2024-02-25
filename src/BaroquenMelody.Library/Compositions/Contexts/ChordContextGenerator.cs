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

#pragma warning disable CS8509 // This switch statement doesn't need to handle values less than 0, so the case for < 0 can be removed.
                var noteSpan = Math.Abs(previousNote.NoteNumber - currentNote.NoteNumber) switch
#pragma warning restore CS8509
                {
                    0 => NoteSpan.None,
                    > 0 and <= LeapThreshold => NoteSpan.Step,
                    > LeapThreshold => NoteSpan.Leap
                };

                return new NoteContext(voice, currentNote, noteMotion, noteSpan);
            });

        return new ChordContext(noteContexts);
    }
}
