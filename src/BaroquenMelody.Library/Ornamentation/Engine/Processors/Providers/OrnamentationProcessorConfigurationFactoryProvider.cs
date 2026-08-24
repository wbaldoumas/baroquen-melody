using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Providers;

internal sealed class OrnamentationProcessorConfigurationFactoryProvider(
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator
) : IOrnamentationProcessorConfigurationFactoryProvider
{
    public IOrnamentationProcessorConfigurationFactory Get(CompositionConfiguration compositionConfiguration) => new OrnamentationProcessorConfigurationFactory(
        new ChordNumberIdentifier(compositionConfiguration),
        weightedRandomBooleanGenerator,
        compositionConfiguration
    );
}
