using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums.Extensions;
using LazyCart;
using Interval = BaroquenMelody.Library.Compositions.MusicTheory.Enums.Interval;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AvoidDirectIntervals(Interval targetInterval) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count == 0)
        {
            return true;
        }

        var instruments = nextChord.Notes.Select(static note => note.Instrument).ToList();
        var instrumentCombos = new LazyCartesianProduct<Instrument, Instrument>(instruments, instruments);
        var precedingChord = precedingChords[^1];

        for (var i = 0; i < instrumentCombos.Size; ++i)
        {
            var (instrumentA, instrumentB) = instrumentCombos[i];

            if (instrumentA == instrumentB)
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
