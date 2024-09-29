using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Strategies;

/// <summary>
///     Creates <see cref="ICompositionStrategy"/> instances.
/// </summary>
internal interface ICompositionStrategyFactory
{
    /// <summary>
    ///     Create a composition strategy.
    /// </summary>
    /// <param name="compositionConfiguration"> The composition configuration. </param>
    /// <returns> A configured composition strategy. </returns>
    ICompositionStrategy Create(CompositionConfiguration compositionConfiguration);
}
