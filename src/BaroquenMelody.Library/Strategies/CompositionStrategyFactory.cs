using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Rules;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Strategies;

/// <inheritdoc cref="ICompositionStrategyFactory"/>
internal sealed class CompositionStrategyFactory(
    INoteChoiceGenerator noteChoiceGenerator,
    ICompositionRule compositionRule,
    IRandomProvider randomProvider,
    ILogger logger
) : ICompositionStrategyFactory
{
    public ICompositionStrategy Create(CompositionConfiguration compositionConfiguration) => new CompositionStrategy(
        new ForwardCheckingChordChoiceEnumerator(compositionConfiguration, noteChoiceGenerator),
        compositionRule,
        logger,
        compositionConfiguration,
        randomProvider,
        maxLookAheadDepth: compositionConfiguration.MaxLookAheadDepth
    );
}
