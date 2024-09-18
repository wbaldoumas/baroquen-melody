using BaroquenMelody.Library.Compositions.Domain;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AvoidOverDoubling : ICompositionRule
{
    private const int DuetThreshold = 1;

    private const int TrioThreshold = 2;

    private const int QuartetThreshold = 3;

    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        var seenNoteNames = new HashSet<NoteName>(nextChord.Notes.Count);
        var uniqueNoteNameCount = nextChord.Notes.Count(note => seenNoteNames.Add(note.NoteName));

        return nextChord.Notes.Count switch
        {
            2 => uniqueNoteNameCount > DuetThreshold,
            3 => uniqueNoteNameCount >= TrioThreshold,
            4 => uniqueNoteNameCount >= QuartetThreshold,
            _ => true
        };
    }
}
