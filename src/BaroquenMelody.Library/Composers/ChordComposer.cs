using BaroquenMelody.Infrastructure.Logging;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Exceptions;
using BaroquenMelody.Library.Strategies;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Composers;

/// <inheritdoc cref="IChordComposer"/>
internal sealed class ChordComposer(
    ICompositionStrategy compositionStrategy,
    ILogger logger
) : IChordComposer
{
    public BaroquenChord Compose(IReadOnlyList<BaroquenChord> precedingChords)
    {
        var possibleChord = compositionStrategy.GetPossibleChords(precedingChords).MinBy(static _ => ThreadLocalRandom.Next());

        if (possibleChord is not null)
        {
            return possibleChord;
        }

        logger.LogCriticalMessage("No valid chord choices available.");

        throw new NoValidChordChoicesAvailableException();
    }
}
