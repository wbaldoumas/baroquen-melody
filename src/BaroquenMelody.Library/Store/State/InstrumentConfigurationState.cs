using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record InstrumentConfigurationState(IDictionary<Instrument, InstrumentConfiguration> Configurations, IDictionary<Instrument, InstrumentConfiguration> LastUserAppliedConfigurations)
{
    public ISet<InstrumentConfiguration> EnabledConfigurations => Configurations.Values.Where(configuration => configuration.IsEnabled).ToHashSet();

    public InstrumentConfigurationState()
        : this(InstrumentConfiguration.DefaultConfigurations, InstrumentConfiguration.DefaultConfigurations)
    {
    }

    public InstrumentConfiguration? this[Instrument instrument] => Configurations.TryGetValue(instrument, out var configuration) ? configuration : null;
}
