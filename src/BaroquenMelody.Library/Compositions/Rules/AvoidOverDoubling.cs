using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Rules;

internal sealed class AvoidOverDoubling : ICompositionRule
{
    private const int DuetThreshold = 1;

    private const int TrioThreshold = 2;

    private const int QuartetThreshold = 3;

    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        var uniqueNoteNameCount = nextChord.Notes.GroupBy(note => note.Raw.NoteName).Count();

        return nextChord.Notes.Count switch
        {
            2 => uniqueNoteNameCount > DuetThreshold,
            3 => uniqueNoteNameCount >= TrioThreshold,
            4 => uniqueNoteNameCount >= QuartetThreshold,
            _ => true
        };
    }
}
