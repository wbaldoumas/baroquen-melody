using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;

namespace BaroquenMelody.Library.Compositions.Rules;

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
        var notes = compositionConfiguration.Scale.GetNotes();

        foreach (var note in lastChord.Notes)
        {
            var nextNote = nextChord[note.Instrument];

            if (!note.IsDissonantWith(nextNote))
            {
                continue;
            }

            var noteScaleIndex = notes.IndexOf(note.Raw);
            var nextNoteScaleIndex = notes.IndexOf(nextNote.Raw);
            var scaleStepDifference = Math.Abs(noteScaleIndex - nextNoteScaleIndex);

            if (scaleStepDifference > LeapThreshold)
            {
                return true;
            }
        }

        return false;
    }
}
