using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

/// <summary>
///     A factory for providing ornamentation processor configurations.
/// </summary>
internal interface IOrnamentationProcessorConfigurationFactory
{
    /// <summary>
    ///     Retrieve the ornamentation processor configurations for the given ornamentation configuration.
    /// </summary>
    /// <param name="ornamentationConfiguration">The ornamentation configuration.</param>
    /// <returns>The ornamentation processor configurations.</returns>
    IEnumerable<OrnamentationProcessorConfiguration> Create(OrnamentationConfiguration ornamentationConfiguration);
}
