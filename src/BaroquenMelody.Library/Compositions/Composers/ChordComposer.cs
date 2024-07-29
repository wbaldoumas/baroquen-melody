using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Exceptions;
using BaroquenMelody.Library.Infrastructure.Logging;
using BaroquenMelody.Library.Infrastructure.Random;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <inheritdoc cref="IChordComposer"/>
internal sealed class ChordComposer(ICompositionStrategy compositionStrategy, CompositionConfiguration compositionConfiguration, ILogger logger) : IChordComposer
{
    public BaroquenChord Compose(IReadOnlyList<BaroquenChord> precedingChords)
    {
        var possibleChordChoices = compositionStrategy.GetPossibleChordChoices(precedingChords);
        var chordChoice = possibleChordChoices.MinBy(_ => ThreadLocalRandom.Next());

        if (chordChoice != null)
        {
            return precedingChords[^1].ApplyChordChoice(compositionConfiguration.Scale, chordChoice);
        }

        logger.NoValidChordChoicesAvailable();

        throw new NoValidChordChoicesAvailableException();
    }
}
