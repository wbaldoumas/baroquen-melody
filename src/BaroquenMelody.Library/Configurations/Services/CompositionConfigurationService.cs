using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Configurations.Services;

internal sealed class CompositionConfigurationService(
    IDispatcher dispatcher,
    IState<CompositionConfigurationState> compositionConfigurationState,
    IState<InstrumentConfigurationState> instrumentConfigurationState,
    IGroundBassFeasibilityAnalyzer groundBassFeasibilityAnalyzer
) : ICompositionConfigurationService
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

    private static readonly FrozenSet<TextureType> _configurableTextures = EnumUtils<TextureType>.AsEnumerable().ToFrozenSet();

    public IEnumerable<NoteName> ConfigurableRootNotes => _configurableRootNotes;

    public IEnumerable<Mode> ConfigurableScaleModes => _configurableScaleModes;

    public IEnumerable<Meter> ConfigurableMeters => _configurableMeters;

    public IEnumerable<CompositionForm> ConfigurableCompositionForms => _configurableCompositionForms;

    public IEnumerable<TextureType> ConfigurableTextures => _configurableTextures;

    public IEnumerable<GroundBass?> ConfigurableGroundBassPatterns { get; } = [null, .. groundBassFeasibilityAnalyzer.GroundBassBank.Select(static groundBass => (GroundBass?)groundBass)];

    public void Randomize()
    {
        var randomRootNote = _configurableRootNotes.MinBy(_ => ThreadLocalRandom.Next());
        var randomScaleMode = _configurableScaleModes.MinBy(_ => ThreadLocalRandom.Next());
        var randomMeter = _configurableMeters.MinBy(_ => ThreadLocalRandom.Next());
        var randomMinimumMeasures = ThreadLocalRandom.Next(MinRandomMinimumMeasures, MaxRandomMinimumMeasures);
        var tempo = ThreadLocalRandom.Next(MinRandomTempo, MaxRandomTempo);

        // The form is a deliberate structural choice, not a musical parameter to roll dice on: randomizing
        // keeps whatever form the user has selected, and the modulation toggle and the texture follow the
        // same reasoning - whether the piece journeys, and what fabric it wears, are structural preferences
        // the dice leave alone. The ground bass pattern IS a musical parameter, so a ground bass
        // randomization rolls it too - but only among the composer's free draw and the patterns that
        // actually fit the randomized key, since pinning an infeasible pattern would silently turn the
        // randomized composition into a fugue.
        var groundBassPattern = compositionConfigurationState.Value.Form == CompositionForm.GroundBass
            ? RandomizeGroundBassPattern(randomRootNote, randomScaleMode)
            : compositionConfigurationState.Value.GroundBassPattern;

        dispatcher.Dispatch(new UpdateCompositionConfiguration(randomRootNote, randomScaleMode, randomMeter, randomMinimumMeasures, tempo, compositionConfigurationState.Value.Form, groundBassPattern, compositionConfigurationState.Value.GroundBassModulate, compositionConfigurationState.Value.Texture));
    }

    public void Reset() => dispatcher.Dispatch(new UpdateCompositionConfiguration(DefaultRootNote, DefaultMode, DefaultMeter));

    private GroundBass? RandomizeGroundBassPattern(NoteName rootNote, Mode mode)
    {
        List<GroundBass?> candidatePatterns =
        [
            null,
            .. groundBassFeasibilityAnalyzer
                .GetFeasibleGroundBasses(instrumentConfigurationState.Value.EnabledConfigurations, new BaroquenScale(rootNote, mode))
                .Select(static groundBass => (GroundBass?)groundBass)
        ];

        return candidatePatterns[ThreadLocalRandom.Next(candidatePatterns.Count)];
    }
}
