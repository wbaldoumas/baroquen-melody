using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record CompositionConfigurationState(BaroquenScale Scale, Meter Meter, int CompositionLength)
{
    public CompositionConfigurationState()
        : this(BaroquenScale.Parse("C Major"), Meter.FourFour, 25)
    {
    }
}
