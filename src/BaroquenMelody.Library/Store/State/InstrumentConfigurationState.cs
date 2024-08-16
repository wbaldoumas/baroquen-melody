using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record InstrumentConfigurationState(IDictionary<Instrument, InstrumentConfiguration> Configurations)
{
    public ISet<InstrumentConfiguration> Aggregate => Configurations.Values.Where(configuration => configuration.IsEnabled).ToHashSet();

    public InstrumentConfigurationState()
        : this(new Dictionary<Instrument, InstrumentConfiguration>())
    {
    }

    public InstrumentConfiguration? this[Instrument instrument] => Configurations.TryGetValue(instrument, out var configuration) ? configuration : null;
}
