using BaroquenMelody.Infrastructure.Logging;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Exceptions;
using BaroquenMelody.Library.Scoring;
using BaroquenMelody.Library.Strategies;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Composers;

/// <inheritdoc cref="IChordComposer"/>
internal sealed class ChordComposer(
    ICompositionStrategy compositionStrategy,
    IChordSelector chordSelector,
    ILogger logger
) : IChordComposer
{
    public BaroquenChord Compose(IReadOnlyList<BaroquenChord> precedingChords)
    {
        var possibleChord = chordSelector.SelectNextChord(precedingChords, compositionStrategy.GetPossibleChords(precedingChords));

        if (possibleChord is not null)
        {
            return possibleChord;
        }

        logger.LogCriticalMessage("No valid chord choices available.");

        throw new NoValidChordChoicesAvailableException();
    }

    // The look-ahead-vetted candidate set is enumerated exactly once and the pin is a filter over it, so an
    // honored pin can only emit a chord the free path could also have chosen, and an unhonorable pin degrades
    // to the free choice for this beat rather than dead-ending the walk.
    public BaroquenChord Compose(IReadOnlyList<BaroquenChord> precedingChords, BaroquenNote pinnedNote)
    {
        var possibleChords = compositionStrategy.GetPossibleChords(precedingChords).ToList();

        var pinnedChords = possibleChords
            .Where(possibleChord => possibleChord.ContainsInstrument(pinnedNote.Instrument) && possibleChord[pinnedNote.Instrument].Raw == pinnedNote.Raw)
            .ToList();

        var possibleChord = chordSelector.SelectNextChord(precedingChords, pinnedChords.Count > 0 ? pinnedChords : possibleChords);

        if (possibleChord is not null)
        {
            return possibleChord;
        }

        logger.LogCriticalMessage("No valid chord choices available.");

        throw new NoValidChordChoicesAvailableException();
    }
}
