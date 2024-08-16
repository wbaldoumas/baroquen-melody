using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record VoiceConfigurationState(IDictionary<Voice, VoiceConfiguration> Configurations)
{
    public ISet<VoiceConfiguration> Aggregate => Configurations.Values.Where(configuration => configuration.IsEnabled).ToHashSet();

    public VoiceConfigurationState()
        : this(new Dictionary<Voice, VoiceConfiguration>())
    {
    }

    public VoiceConfiguration? this[Voice voice] => Configurations.TryGetValue(voice, out var configuration) ? configuration : null;
}
