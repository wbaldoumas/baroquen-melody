using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Exceptions;
using BaroquenMelody.Library.Infrastructure.Logging;
using BaroquenMelody.Library.Infrastructure.Random;
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

        logger.NoValidChordChoicesAvailable();

        throw new NoValidChordChoicesAvailableException();
    }
}
