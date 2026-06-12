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
}
