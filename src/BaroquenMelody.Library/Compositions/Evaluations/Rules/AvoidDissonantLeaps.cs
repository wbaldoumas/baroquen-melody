using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Evaluations.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AvoidDissonantLeaps(CompositionConfiguration compositionConfiguration) : ICompositionRule
{
    private const int LeapThreshold = 1;

    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count == 0)
        {
            return true;
        }

        return !IsDissonantLeap(precedingChords[^1], nextChord);
    }

    private bool IsDissonantLeap(BaroquenChord lastChord, BaroquenChord nextChord)
    {
        var notes = compositionConfiguration.Scale.GetNotes().ToList();

        return (
            from note in lastChord.Notes
            let nextNote = nextChord[note.Voice]
            where note.IsDissonantWith(nextNote)
            let noteScaleIndex = notes.IndexOf(note.Raw)
            let nextNoteScaleIndex = notes.IndexOf(nextNote.Raw)
            select Math.Abs(noteScaleIndex - nextNoteScaleIndex)
        ).Any(scaleStepDifference => scaleStepDifference > LeapThreshold);
    }
}
