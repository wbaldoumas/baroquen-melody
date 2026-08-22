using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
[ExcludeFromCodeCoverage(Justification = "Simple record without logic")]
public sealed record CompositionConfigurationState(NoteName TonicNote, Mode Mode, Meter Meter, int MinimumMeasures = 25, int Tempo = 120, CompositionForm Form = CompositionForm.Fugue, GroundBass? GroundBassPattern = null, bool GroundBassModulate = true, TextureType Texture = TextureType.None)
{
    public BaroquenScale Scale { get; } = new(TonicNote, Mode);

    public IList<Note> AvailableNotes { get; } = new BaroquenScale(TonicNote, Mode).AvailableNotes;

    public CompositionConfigurationState()
        : this(NoteName.C, Mode.Ionian, Meter.FourFour)
    {
    }
}
