using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Scoring;

/// <summary>
///     A factory for creating <see cref="IScoringRule"/>s from configurations.
/// </summary>
internal interface IScoringRuleFactory
{
    /// <summary>
    ///     Creates a weighted <see cref="IScoringRule"/> from the given configuration.
    /// </summary>
    /// <param name="configuration">The configuration to create the scoring rule from.</param>
    /// <returns>The weighted scoring rule.</returns>
    public IScoringRule Create(ScoringRuleConfiguration configuration);

    /// <summary>
    ///     Creates an aggregate <see cref="IScoringRule"/> from the enabled, positively-weighted configurations.
    /// </summary>
    /// <param name="aggregateConfiguration">The aggregate configuration to create the scoring rule from.</param>
    /// <returns>The aggregate scoring rule.</returns>
    public IScoringRule CreateAggregate(AggregateScoringRuleConfiguration aggregateConfiguration);
}
