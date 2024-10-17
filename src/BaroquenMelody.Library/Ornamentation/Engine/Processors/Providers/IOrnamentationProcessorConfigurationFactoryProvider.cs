using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Providers;

/// <summary>
///     A provider for creating ornamentation processor configuration factories.
/// </summary>
internal interface IOrnamentationProcessorConfigurationFactoryProvider
{
    /// <summary>
    ///     Gets a new ornamentation processor configuration factory.
    /// </summary>
    /// <param name="compositionConfiguration">The current composition configuration.</param>
    /// <returns>The new ornamentation processor configuration factory.</returns>
    IOrnamentationProcessorConfigurationFactory Get(CompositionConfiguration compositionConfiguration);
}
