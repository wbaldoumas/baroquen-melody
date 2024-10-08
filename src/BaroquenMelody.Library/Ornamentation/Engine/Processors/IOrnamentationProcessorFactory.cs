using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors;

/// <summary>
///     A factory for creating <see cref="IProcessor{OrnamentationItem}"/>s for <see cref="OrnamentationType"/>s.
/// </summary>
internal interface IOrnamentationProcessorFactory
{
    /// <summary>
    ///     Creates a new <see cref="IProcessor{OrnamentationItem}"/> for the given <see cref="OrnamentationType"/>.
    /// </summary>
    /// <param name="ornamentationType">The <see cref="OrnamentationType"/> to create the <see cref="IProcessor{OrnamentationItem}"/> for.</param>
    /// <param name="compositionConfiguration">The current <see cref="CompositionConfiguration"/> to create the <see cref="IProcessor{OrnamentationItem}"/> for.</param>
    /// <param name="interval">The interval to create the <see cref="IProcessor{OrnamentationItem}"/> for.</param>
    /// <returns>A new <see cref="IProcessor{OrnamentationItem}"/> for the given <see cref="OrnamentationType"/>.</returns>
    IProcessor<OrnamentationItem> Create(OrnamentationType ornamentationType, CompositionConfiguration compositionConfiguration, int interval = 0);
}
