using BaroquenMelody.Infrastructure.Collections.Extensions;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class FollowsStandardProgression : ICompositionRule
{
    private readonly FrozenSet<(int, int)> _validProgressionHashes;

    /// <inheritdoc cref="ICompositionRule"/>
    public FollowsStandardProgression(CompositionConfiguration compositionConfiguration)
    {
        List<(FrozenSet<NoteName>, FrozenSet<NoteName>)> validProgressions =
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

        var validProgressionHashes = new HashSet<(int, int)>();

        foreach (var (precedingChord, nextChord) in validProgressions)
        {
            PreProcessProgressionHashes(precedingChord, nextChord, ref validProgressionHashes);
        }

        _validProgressionHashes = validProgressionHashes.ToFrozenSet();
    }

    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count == 0)
        {
            return true;
        }

        var precedingChordHash = precedingChords[^1].Notes
            .Select(static note => note.NoteName)
            .ToHashSet()
            .GetContentHashCode();

        var nextChordHash = nextChord.Notes
            .Select(static note => note.NoteName)
            .ToHashSet()
            .GetContentHashCode();

        return _validProgressionHashes.Contains((precedingChordHash, nextChordHash));
    }

    private static void PreProcessProgressionHashes(
        FrozenSet<NoteName> precedingChord,
        FrozenSet<NoteName> nextChord,
        ref HashSet<(int, int)> validProgressions)
    {
        var precedingPowerSet = precedingChord.ToPowerSet();
        var nextPowerSet = nextChord.ToPowerSet().ToList();

        foreach (var precedingSet in precedingPowerSet)
        {
            foreach (var nextSet in nextPowerSet)
            {
                validProgressions.Add((precedingSet.GetContentHashCode(), nextSet.GetContentHashCode()));
            }
        }
    }
}
