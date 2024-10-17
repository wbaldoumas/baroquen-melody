using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

/// <summary>
///     Creates ornamentation processors for the given composition configuration.
/// </summary>
internal interface IOrnamentationProcessorFactory
{
    /// <summary>
    ///     Create all the ornamentation processors for the given composition configuration, with their
    ///     input and output policies in place.
    /// </summary>
    /// <param name="compositionConfiguration">The current composition configuration.</param>
    /// <returns>All the ornamentation processors for the given composition configuration.</returns>
    IEnumerable<IProcessor<OrnamentationItem>> Create(CompositionConfiguration compositionConfiguration);
}
