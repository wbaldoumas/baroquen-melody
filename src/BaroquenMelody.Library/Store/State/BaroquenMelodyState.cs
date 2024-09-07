using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record BaroquenMelodyState(BaroquenMelody? BaroquenMelody, string Path, bool HasBeenSaved)
{
    public BaroquenMelodyState()
        : this(null, string.Empty, false)
    {
    }
}
