using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AvoidDissonance : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        var notes = nextChord.Notes.Select(note => note.Raw).DistinctBy(note => note.NoteName).ToList();

        return notes.TrueForAll(note => notes.TrueForAll(otherNote => !note.IsDissonantWith(otherNote)));
    }
}
