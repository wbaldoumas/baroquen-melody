using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Rules;

/// <summary>
///     Creates <see cref="ICompositionRule"/> instances.
/// </summary>
internal interface ICompositionRuleFactory
{
    /// <summary>
    ///     Create a composition rule.
    /// </summary>
    /// <param name="configuration">The composition rule configuration.</param>
    /// <returns>The composition rule.</returns>
    public ICompositionRule Create(CompositionRuleConfiguration configuration);
}
