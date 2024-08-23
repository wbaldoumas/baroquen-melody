using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
[ExcludeFromCodeCoverage(Justification = "Simple record without logic")]
public sealed record CompositionConfigurationState(NoteName TonicNote, Mode Mode, Meter Meter, int MinimumMeasures = 25)
{
    public BaroquenScale Scale => new(TonicNote, Mode);

    public CompositionConfigurationState()
        : this(NoteName.C, Mode.Ionian, Meter.FourFour)
    {
    }
}
