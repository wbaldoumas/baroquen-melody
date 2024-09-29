using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.MusicTheory.Enums.Extensions;

namespace BaroquenMelody.Library.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AvoidParallelIntervals(Interval targetInterval) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count == 0)
        {
            return true;
        }

        return !HasParallelIntervals(precedingChords[^1], nextChord);
    }

    private bool HasParallelIntervals(BaroquenChord lastChord, BaroquenChord nextChord)
    {
        foreach (var (instrumentA, instrumentB) in GetParallelPerfectInstrumentPairs(lastChord))
        {
            var nextNoteA = nextChord[instrumentA];
            var nextNoteB = nextChord[instrumentB];

            if (IntervalExtensions.FromNotes(nextNoteA, nextNoteB) == targetInterval && lastChord.InstrumentsMoveInParallel(nextChord, instrumentA, instrumentB))
            {
                return true;
            }
        }

        return false;
    }

    private List<(Instrument, Instrument)> GetParallelPerfectInstrumentPairs(BaroquenChord lastChord)
    {
        var parallelPerfectInstrumentPairs = new List<(Instrument, Instrument)>();

        foreach (var note in lastChord.Notes)
        {
            foreach (var otherNote in lastChord.Notes)
            {
                if (note == otherNote)
                {
                    continue;
                }

                if (IntervalExtensions.FromNotes(note, otherNote) == targetInterval)
                {
                    parallelPerfectInstrumentPairs.Add((note.Instrument, otherNote.Instrument));
                }
            }
        }

        return parallelPerfectInstrumentPairs;
    }
}
