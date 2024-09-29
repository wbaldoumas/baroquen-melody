using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
using Fluxor;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record CompositionOrnamentationConfigurationState(IDictionary<OrnamentationType, OrnamentationConfiguration> Configurations)
{
    private static readonly IDictionary<OrnamentationType, OrnamentationConfiguration> Defaults = AggregateOrnamentationConfiguration.Default.Configurations.ToDictionary(
        configuration => configuration.OrnamentationType,
        configuration => configuration
    );

    public AggregateOrnamentationConfiguration Aggregate => new(Configurations.Values.ToFrozenSet());

    public CompositionOrnamentationConfigurationState()
        : this(Defaults)
    {
    }

    public OrnamentationConfiguration? this[OrnamentationType ornamentationType] => Configurations.TryGetValue(ornamentationType, out var configuration) ? configuration : null;
}
