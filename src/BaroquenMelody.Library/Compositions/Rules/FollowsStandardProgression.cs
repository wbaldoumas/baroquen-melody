using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Rules;

internal sealed class FollowsStandardProgression(CompositionConfiguration compositionConfiguration) : ICompositionRule
{
    private readonly List<(HashSet<NoteName>, HashSet<NoteName>)> _validMajorProgressions =
    [
        (compositionConfiguration.Scale.I, compositionConfiguration.Scale.I),
        (compositionConfiguration.Scale.I, compositionConfiguration.Scale.II),
        (compositionConfiguration.Scale.I, compositionConfiguration.Scale.III),
        (compositionConfiguration.Scale.I, compositionConfiguration.Scale.IV),
        (compositionConfiguration.Scale.I, compositionConfiguration.Scale.V),
        (compositionConfiguration.Scale.I, compositionConfiguration.Scale.VI),
        (compositionConfiguration.Scale.I, compositionConfiguration.Scale.VII),

        (compositionConfiguration.Scale.II, compositionConfiguration.Scale.I),
        (compositionConfiguration.Scale.II, compositionConfiguration.Scale.II),
        (compositionConfiguration.Scale.II, compositionConfiguration.Scale.III),
        (compositionConfiguration.Scale.II, compositionConfiguration.Scale.V),
        (compositionConfiguration.Scale.II, compositionConfiguration.Scale.VI),
        (compositionConfiguration.Scale.II, compositionConfiguration.Scale.VII),

        (compositionConfiguration.Scale.III, compositionConfiguration.Scale.II),
        (compositionConfiguration.Scale.III, compositionConfiguration.Scale.III),
        (compositionConfiguration.Scale.III, compositionConfiguration.Scale.IV),
        (compositionConfiguration.Scale.III, compositionConfiguration.Scale.VI),

        (compositionConfiguration.Scale.IV, compositionConfiguration.Scale.I),
        (compositionConfiguration.Scale.IV, compositionConfiguration.Scale.III),
        (compositionConfiguration.Scale.IV, compositionConfiguration.Scale.IV),
        (compositionConfiguration.Scale.IV, compositionConfiguration.Scale.V),
        (compositionConfiguration.Scale.IV, compositionConfiguration.Scale.VII),

        (compositionConfiguration.Scale.V, compositionConfiguration.Scale.I),
        (compositionConfiguration.Scale.V, compositionConfiguration.Scale.V),
        (compositionConfiguration.Scale.V, compositionConfiguration.Scale.VI),

        (compositionConfiguration.Scale.VI, compositionConfiguration.Scale.II),
        (compositionConfiguration.Scale.VI, compositionConfiguration.Scale.IV),
        (compositionConfiguration.Scale.VI, compositionConfiguration.Scale.VI),

        (compositionConfiguration.Scale.VII, compositionConfiguration.Scale.I),
        (compositionConfiguration.Scale.VII, compositionConfiguration.Scale.III),
        (compositionConfiguration.Scale.VII, compositionConfiguration.Scale.VI),
        (compositionConfiguration.Scale.VII, compositionConfiguration.Scale.VII)
    ];

    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count == 0)
        {
            return true;
        }

        var precedingChordNoteNames = precedingChords[^1].Notes.Select(note => note.Raw.NoteName).ToHashSet();
        var nextChordNoteNames = nextChord.Notes.Select(note => note.Raw.NoteName).ToHashSet();

        return IsValidProgression(precedingChordNoteNames, nextChordNoteNames);
    }

    private bool IsValidProgression(HashSet<NoteName> precedingChordNoteNames, HashSet<NoteName> nextChordNoteNames)
    {
        foreach (var (validPrecedingChordNoteNames, validNextChordNoteNames) in _validMajorProgressions)
        {
            if (precedingChordNoteNames.IsSubsetOf(validPrecedingChordNoteNames) && nextChordNoteNames.IsSubsetOf(validNextChordNoteNames))
            {
                return true;
            }
        }

        return false;
    }
}
