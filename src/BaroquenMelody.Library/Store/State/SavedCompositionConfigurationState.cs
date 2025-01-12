using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record SavedCompositionConfigurationState(string LastLoadedConfigurationName)
{
    public SavedCompositionConfigurationState()
        : this(string.Empty)
    {
    }
}
