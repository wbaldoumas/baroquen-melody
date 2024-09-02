using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record BaroquenMelodyState(BaroquenMelody? BaroquenMelody)
{
    public BaroquenMelodyState()
        : this(null as BaroquenMelody)
    {
    }
}
