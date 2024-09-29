using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Store.Actions;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Compositions.Configurations.Services;

internal sealed class CompositionConfigurationService(IDispatcher dispatcher) : ICompositionConfigurationService
{
    private const Meter _defaultMeter = Meter.FourFour;

    private const NoteName _defaultRootNote = NoteName.C;

    private const Mode _defaultMode = Mode.Ionian;

    private const int _minRandomMinimumMeasures = 10;

    private const int _maxRandomMinimumMeasures = 100;

    private const int _minRandomTempo = 40;

    private const int _maxRandomTempo = 250;

    private static readonly FrozenSet<NoteName> _configurableRootNotes = EnumUtils<NoteName>.AsEnumerable().ToFrozenSet();

    private static readonly FrozenSet<Mode> _configurableScaleModes = EnumUtils<Mode>.AsEnumerable().ToFrozenSet();

    private static readonly FrozenSet<Meter> _configurableMeters = EnumUtils<Meter>.AsEnumerable().ToFrozenSet();

    public IEnumerable<NoteName> ConfigurableRootNotes => _configurableRootNotes;

    public IEnumerable<Mode> ConfigurableScaleModes => _configurableScaleModes;

    public IEnumerable<Meter> ConfigurableMeters => _configurableMeters;

    public void Randomize()
    {
        var randomRootNote = _configurableRootNotes.MinBy(_ => ThreadLocalRandom.Next());
        var randomScaleMode = _configurableScaleModes.MinBy(_ => ThreadLocalRandom.Next());
        var randomMeter = _configurableMeters.MinBy(_ => ThreadLocalRandom.Next());
        var randomMinimumMeasures = ThreadLocalRandom.Next(_minRandomMinimumMeasures, _maxRandomMinimumMeasures);
        var tempo = ThreadLocalRandom.Next(_minRandomTempo, _maxRandomTempo);

        dispatcher.Dispatch(new UpdateCompositionConfiguration(randomRootNote, randomScaleMode, randomMeter, randomMinimumMeasures, tempo));
    }

    public void Reset() => dispatcher.Dispatch(new UpdateCompositionConfiguration(_defaultRootNote, _defaultMode, _defaultMeter));
}
