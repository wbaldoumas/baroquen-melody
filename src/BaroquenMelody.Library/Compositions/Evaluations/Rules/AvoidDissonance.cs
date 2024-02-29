using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;

namespace BaroquenMelody.Library.Compositions.Evaluations.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AvoidDissonance : ICompositionRule
{
    public bool Evaluate(ContextualizedChord currentChord, ContextualizedChord nextChord)
    {
        var notes = nextChord.ContextualizedNotes.Select(contextualizedNote => contextualizedNote.Note).ToHashSet();

        return notes.All(note => notes.Where(otherNote => otherNote != note).All(otherNote => !note.IsDissonantWith(otherNote)));
    }
}
