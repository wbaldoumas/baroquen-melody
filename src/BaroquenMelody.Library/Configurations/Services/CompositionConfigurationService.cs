using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Configurations.Services;

internal sealed class CompositionConfigurationService(IDispatcher dispatcher, IState<CompositionConfigurationState> compositionConfigurationState) : ICompositionConfigurationService
{
    private const Meter DefaultMeter = Meter.FourFour;

    private const NoteName DefaultRootNote = NoteName.C;

    private const Mode DefaultMode = Mode.Ionian;

    private const int MinRandomMinimumMeasures = 10;

    private const int MaxRandomMinimumMeasures = 100;

    private const int MinRandomTempo = 40;

    private const int MaxRandomTempo = 250;

    private static readonly FrozenSet<NoteName> _configurableRootNotes = EnumUtils<NoteName>.AsEnumerable().ToFrozenSet();

    private static readonly FrozenSet<Mode> _configurableScaleModes = EnumUtils<Mode>.AsEnumerable().ToFrozenSet();

    private static readonly FrozenSet<Meter> _configurableMeters = EnumUtils<Meter>.AsEnumerable().ToFrozenSet();

    private static readonly FrozenSet<CompositionForm> _configurableCompositionForms = EnumUtils<CompositionForm>.AsEnumerable().ToFrozenSet();

    public IEnumerable<NoteName> ConfigurableRootNotes => _configurableRootNotes;

    public IEnumerable<Mode> ConfigurableScaleModes => _configurableScaleModes;

    public IEnumerable<Meter> ConfigurableMeters => _configurableMeters;

    public IEnumerable<CompositionForm> ConfigurableCompositionForms => _configurableCompositionForms;

    public void Randomize()
    {
        var randomRootNote = _configurableRootNotes.MinBy(_ => ThreadLocalRandom.Next());
        var randomScaleMode = _configurableScaleModes.MinBy(_ => ThreadLocalRandom.Next());
        var randomMeter = _configurableMeters.MinBy(_ => ThreadLocalRandom.Next());
        var randomMinimumMeasures = ThreadLocalRandom.Next(MinRandomMinimumMeasures, MaxRandomMinimumMeasures);
        var tempo = ThreadLocalRandom.Next(MinRandomTempo, MaxRandomTempo);

        // The form is a deliberate structural choice, not a musical parameter to roll dice on: randomizing
        // keeps whatever form the user has selected.
        dispatcher.Dispatch(new UpdateCompositionConfiguration(randomRootNote, randomScaleMode, randomMeter, randomMinimumMeasures, tempo, compositionConfigurationState.Value.Form));
    }

    public void Reset() => dispatcher.Dispatch(new UpdateCompositionConfiguration(DefaultRootNote, DefaultMode, DefaultMeter));
}
