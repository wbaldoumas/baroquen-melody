using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Providers;

internal interface IOrnamentationProcessorConfigurationFactoryProvider
{
    IOrnamentationProcessorConfigurationFactory Get(CompositionConfiguration compositionConfiguration);
}
