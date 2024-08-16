using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record CompositionOrnamentationConfigurationState(IDictionary<OrnamentationType, OrnamentationConfiguration> Configurations)
{
    public AggregateOrnamentationConfiguration Aggregate => new(Configurations.Values.ToHashSet());

    public CompositionOrnamentationConfigurationState()
        : this(new Dictionary<OrnamentationType, OrnamentationConfiguration>())
    {
    }

    public OrnamentationConfiguration? this[OrnamentationType ornamentationType] => Configurations.TryGetValue(ornamentationType, out var configuration) ? configuration : null;
}
