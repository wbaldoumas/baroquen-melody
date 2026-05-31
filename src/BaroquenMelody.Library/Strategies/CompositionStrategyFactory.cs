using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Rules;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Strategies;

/// <inheritdoc cref="ICompositionStrategyFactory"/>
internal sealed class CompositionStrategyFactory(
    IChordChoiceRepositoryFactory chordChoiceRepositoryFactory,
    ICompositionRule compositionRule,
    IRandomProvider randomProvider,
    ILogger logger
) : ICompositionStrategyFactory
{
    public ICompositionStrategy Create(CompositionConfiguration compositionConfiguration) => new CompositionStrategy(
        chordChoiceRepositoryFactory.Create(compositionConfiguration),
        compositionRule,
        logger,
        compositionConfiguration,
        randomProvider,
        maxLookAheadDepth: compositionConfiguration.MaxLookAheadDepth
    );
}
