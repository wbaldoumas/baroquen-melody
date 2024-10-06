using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record BaroquenMelodyState(MidiFileComposition? Composition, string Path, bool HasBeenSaved)
{
    public BaroquenMelodyState()
        : this(null, string.Empty, false)
    {
    }
}
