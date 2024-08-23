using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record CompositionConfigurationState(NoteName RootNote, Mode Mode, Meter Meter, int CompositionLength = 25)
{
    public BaroquenScale Scale => new(RootNote, Mode);

    public CompositionConfigurationState()
        : this(NoteName.C, Mode.Ionian, Meter.FourFour)
    {
    }
}
