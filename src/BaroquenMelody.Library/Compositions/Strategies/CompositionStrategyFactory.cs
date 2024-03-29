﻿using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;

namespace BaroquenMelody.Library.Compositions.Strategies;

internal sealed class CompositionStrategyFactory(
    IChordChoiceRepositoryFactory chordChoiceRepositoryFactory,
    ICompositionRule compositionRule
) : ICompositionStrategyFactory
{
    public ICompositionStrategy Create(CompositionConfiguration compositionConfiguration) => new CompositionStrategy(
        chordChoiceRepositoryFactory.Create(compositionConfiguration),
        compositionRule,
        compositionConfiguration
    );
}
