using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Rules;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Compositions.Strategies;

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
