using BaroquenMelody.Infrastructure.Logging;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Exceptions;
using BaroquenMelody.Library.Compositions.Strategies;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Compositions.Composers;

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
