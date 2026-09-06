using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

/// <summary>
///     The pedal figures: a chord tone or the octave held as a returning pedal while the principal moves against it.
///     The degree-gated pedals come one per chord degree, the octave pedals one per direction, each with the
///     moving-interior (passing tone, arpeggio) and static-interior variants.
/// </summary>
internal sealed partial class OrnamentationProcessorConfigurationFactory
{
    private const int DoublePedalPassingToneInterval = 4;

    private const int RootPedalInterval = -3;

    private const int ThirdPedalInterval = -2;

    private const int FifthPedalInterval = -4;

    private IEnumerable<OrnamentationProcessorConfiguration> CreatePedals(OrnamentationConfiguration configuration) =>
    [
        CreatePedal(_isRootOfChord, RootPedalInterval, configuration),
        CreatePedal(_isThirdOfChord, ThirdPedalInterval, configuration),
        CreatePedal(_isFifthOfChord, FifthPedalInterval, configuration)
    ];

    private OrnamentationProcessorConfiguration CreatePedal(
        IInputPolicy<OrnamentationItem> scaleDegreePolicy,
        int pedalInterval,
        OrnamentationConfiguration configuration
    ) => new(
        OrnamentationType.Pedal,
        InputPolicies:
        [
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            scaleDegreePolicy,
            new IsApplicableInterval(_compositionConfiguration, interval: 2),
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.Pedal)),
            new IsIntervalWithinInstrumentRange(_compositionConfiguration, pedalInterval)
        ],
        OutputPolicies: [new LogOrnamentation(configuration.OrnamentationType, _logger)],
        Translations: [pedalInterval, 1, pedalInterval],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 1 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateDoublePedalPassingTone(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DoublePedalPassingTone,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, DoublePedalPassingToneInterval)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [0, 1, 0, 2, 0, 3, 0],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 1, 3, 5 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateOctavePedal(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.OctavePedal,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsIntervalWithinInstrumentRange(_compositionConfiguration, -7),
            new Not<OrnamentationItem>(new IsApplicableInterval(_compositionConfiguration, PassingToneInterval)),
            new Not<OrnamentationItem>(new IsApplicableInterval(_compositionConfiguration, 4))
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [-7, 0, -7],
        ShouldNotInvert,
        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateOctavePedalPassingTone(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.OctavePedalPassingTone,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsIntervalWithinInstrumentRange(_compositionConfiguration, -7),
            new IsApplicableInterval(_compositionConfiguration, PassingToneInterval).Or(_isRepeatedNote)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [-7, 1, -7],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 1 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateOctavePedalArpeggio(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.OctavePedalArpeggio,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            _hasNextBeat,
            _isAscending,
            new IsIntervalWithinInstrumentRange(_compositionConfiguration, -7),
            new IsApplicableInterval(_compositionConfiguration, 4).Or(_isRepeatedNote),
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [-7, 2, -7],
        ShouldNotInvert,
        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateUpperOctavePedal(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.UpperOctavePedal,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsIntervalWithinInstrumentRange(_compositionConfiguration, 7),
            new Not<OrnamentationItem>(new IsApplicableInterval(_compositionConfiguration, PassingToneInterval)),
            new Not<OrnamentationItem>(new IsApplicableInterval(_compositionConfiguration, 4))
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [7, 0, 7],
        ShouldNotInvert,
        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateUpperOctavePedalPassingTone(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.UpperOctavePedalPassingTone,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsIntervalWithinInstrumentRange(_compositionConfiguration, 7),
            new IsApplicableInterval(_compositionConfiguration, PassingToneInterval).Or(_isRepeatedNote)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [7, 1, 7],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 1 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateUpperOctavePedalArpeggio(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.UpperOctavePedalArpeggio,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            _hasNextBeat,
            _isDescending,
            new IsIntervalWithinInstrumentRange(_compositionConfiguration, 7),
            new IsApplicableInterval(_compositionConfiguration, 4).Or(_isRepeatedNote),
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [7, 2, 7],
        ShouldNotInvert,
        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
    );
}
