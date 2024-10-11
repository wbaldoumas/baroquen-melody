using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

internal interface IOrnamentationProcessorConfigurationFactory
{
    IEnumerable<OrnamentationProcessorConfiguration> Create(OrnamentationConfiguration ornamentationConfiguration);
}
