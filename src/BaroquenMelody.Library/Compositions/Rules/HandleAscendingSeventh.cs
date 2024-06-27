using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class HandleAscendingSeventh(CompositionConfiguration compositionConfiguration) : ICompositionRule
{
    private const int SeventhScaleDegree = 6;

    private readonly NoteName _seventhScaleDegreeNoteName = compositionConfiguration.Scale.Raw.GetStep(SeventhScaleDegree);

    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count < 2)
        {
            return true;
        }

        var nextToLastChord = precedingChords[^2];
        var lastChord = precedingChords[^1];

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var seventhNote in lastChord.Notes.Where(note => note.Raw.NoteName == _seventhScaleDegreeNoteName))
        {
            var nextToLastNote = nextToLastChord[seventhNote.Voice];

            if (nextToLastNote.Raw.NoteNumber >= seventhNote.Raw.NoteNumber)
            {
                continue;
            }

            var nextNote = nextChord[seventhNote.Voice];

            if (nextNote.Raw.NoteNumber <= seventhNote.Raw.NoteNumber)
            {
                return false;
            }
        }

        return true;
    }
}
