using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.MusicTheory.Enums.Extensions;
using Interval = BaroquenMelody.Library.MusicTheory.Enums.Interval;

namespace BaroquenMelody.Library.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AvoidDirectIntervals(
    Interval targetInterval,
    CompositionConfiguration compositionConfiguration
) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count == 0)
        {
            return true;
        }

        var precedingChord = precedingChords[^1];

        for (var i = 0; i < compositionConfiguration.InstrumentPairs.Size; ++i)
        {
            var (instrumentA, instrumentB) = compositionConfiguration.InstrumentPairs[i];

            if (instrumentA == instrumentB || !nextChord.ContainsInstrument(instrumentA) || !nextChord.ContainsInstrument(instrumentB))
            {
                continue;
            }

            if (HaveTargetInterval(nextChord, instrumentA, instrumentB, targetInterval) && precedingChord.InstrumentsMoveInParallel(nextChord, instrumentA, instrumentB))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HaveTargetInterval(BaroquenChord nextChord, Instrument instrumentA, Instrument instrumentB, Interval targetInterval)
    {
        var nextNoteA = nextChord[instrumentA];
        var nextNoteB = nextChord[instrumentB];

        return IntervalExtensions.FromNotes(nextNoteA, nextNoteB) == targetInterval;
    }
}
