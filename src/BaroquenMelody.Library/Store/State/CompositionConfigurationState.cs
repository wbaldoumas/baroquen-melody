using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Infrastructure.Collections.Extensions;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
[ExcludeFromCodeCoverage(Justification = "Simple record without logic")]
public sealed record CompositionConfigurationState(NoteName TonicNote, Mode Mode, Meter Meter, int MinimumMeasures = 25, int Tempo = 120)
{
    public BaroquenScale Scale { get; } = new(TonicNote, Mode);

    public IList<Note> Notes { get; } = new BaroquenScale(TonicNote, Mode).GetNotes().TrimEdges(CompositionConfiguration.MaxScaleStepChange).ToList();

    public CompositionConfigurationState()
        : this(NoteName.C, Mode.Ionian, Meter.FourFour)
    {
    }
}
