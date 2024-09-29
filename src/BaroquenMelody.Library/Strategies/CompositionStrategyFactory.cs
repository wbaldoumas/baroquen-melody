using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Rules;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Strategies;

/// <inheritdoc cref="ICompositionStrategyFactory"/>
internal sealed class CompositionStrategyFactory(
    IChordChoiceRepositoryFactory chordChoiceRepositoryFactory,
    ICompositionRule compositionRule,
    ILogger logger
) : ICompositionStrategyFactory
{
    public ICompositionStrategy Create(CompositionConfiguration compositionConfiguration) => new CompositionStrategy(
        chordChoiceRepositoryFactory.Create(compositionConfiguration),
        compositionRule,
        logger,
        compositionConfiguration,
        maxLookAheadDepth: 1
    );
}
