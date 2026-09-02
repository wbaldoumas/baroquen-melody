using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record CompositionOrnamentationConfigurationState(IDictionary<OrnamentationType, OrnamentationConfiguration> Configurations)
{
    private static readonly IDictionary<OrnamentationType, OrnamentationConfiguration> Defaults = AggregateOrnamentationConfiguration.Default.Configurations.ToDictionary(
        configuration => configuration.OrnamentationType,
        configuration => configuration
    );

    // An insertion-ordered HashSet, never a frozen set: a frozen set of records enumerates in hash-bucket order,
    // which varies with process history.
    public AggregateOrnamentationConfiguration Aggregate => new(Configurations.Values.ToHashSet());

    public CompositionOrnamentationConfigurationState()
        : this(Defaults)
    {
    }

    public OrnamentationConfiguration? this[OrnamentationType ornamentationType] => Configurations.TryGetValue(ornamentationType, out var configuration) ? configuration : null;
}
