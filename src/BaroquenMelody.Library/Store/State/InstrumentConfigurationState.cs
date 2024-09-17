using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record InstrumentConfigurationState(IDictionary<Instrument, InstrumentConfiguration> Configurations, IDictionary<Instrument, InstrumentConfiguration> LastUserAppliedConfigurations)
{
    private const int MinimumEnabledConfigurations = 1;

    public ISet<InstrumentConfiguration> EnabledConfigurations => Configurations.Values.Where(configuration => configuration.IsEnabled).ToHashSet();

    public ISet<InstrumentConfiguration> AllConfigurations => Configurations.Values.ToHashSet();

    public InstrumentConfigurationState()
        : this(InstrumentConfiguration.DefaultConfigurations, InstrumentConfiguration.DefaultConfigurations)
    {
    }

    public InstrumentConfiguration? this[Instrument instrument] => Configurations.TryGetValue(instrument, out var configuration) ? configuration : null;

    public bool IsValid => EnabledConfigurations.Count >= MinimumEnabledConfigurations;

    public string ValidationMessage => IsValid ? string.Empty : "Cannot compose. Invalid instrumentation.";
}
