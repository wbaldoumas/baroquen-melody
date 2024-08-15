using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record VoiceConfigurationState(IDictionary<Voice, VoiceConfiguration> Configurations)
{
    public ISet<VoiceConfiguration> Aggregate => Configurations.Values.ToHashSet();

    public VoiceConfigurationState()
        : this(new Dictionary<Voice, VoiceConfiguration>())
    {
    }
}
