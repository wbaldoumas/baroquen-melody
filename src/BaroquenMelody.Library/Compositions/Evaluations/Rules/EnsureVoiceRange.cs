using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Evaluations.Rules;

internal sealed class EnsureVoiceRange(CompositionConfiguration configuration) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord) =>
        nextChord.Notes.All(note => configuration.IsNoteInVoiceRange(note.Voice, note.Raw));
}
