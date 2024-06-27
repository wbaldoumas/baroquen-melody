using BaroquenMelody.Library.Compositions.Configurations;

namespace BaroquenMelody.Library.Compositions.Strategies;

internal interface ICompositionStrategyFactory
{
    /// <summary>
    ///     Create a composition strategy.
    /// </summary>
    /// <param name="compositionConfiguration"> The composition configuration. </param>
    /// <returns> A configured composition strategy. </returns>
    // ReSharper disable once UnusedMemberInSuper.Global
    ICompositionStrategy Create(CompositionConfiguration compositionConfiguration);
}
