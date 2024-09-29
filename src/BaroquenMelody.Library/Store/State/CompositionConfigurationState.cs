using BaroquenMelody.Infrastructure.Collections.Extensions;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
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
