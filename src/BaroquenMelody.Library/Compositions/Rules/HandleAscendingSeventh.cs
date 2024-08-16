using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class HandleAscendingSeventh(CompositionConfiguration compositionConfiguration) : ICompositionRule
{
    private const int MinimumPrecedingChords = 2;

    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count < MinimumPrecedingChords)
        {
            return true;
        }

        var nextToLastChord = precedingChords[^2];
        var lastChord = precedingChords[^1];

        foreach (var seventhNote in lastChord.Notes.Where(note => note.NoteName == compositionConfiguration.Scale.LeadingTone))
        {
            var nextToLastNote = nextToLastChord[seventhNote.Instrument];

            if (nextToLastNote.NoteNumber >= seventhNote.NoteNumber)
            {
                continue;
            }

            var nextNote = nextChord[seventhNote.Instrument];

            if (nextNote.NoteNumber <= seventhNote.NoteNumber)
            {
                return false;
            }
        }

        return true;
    }
}
