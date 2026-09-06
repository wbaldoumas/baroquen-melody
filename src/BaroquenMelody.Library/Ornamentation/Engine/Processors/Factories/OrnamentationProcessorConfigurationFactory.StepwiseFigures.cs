using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

/// <summary>
///     The stepwise figures: passing tones and runs, which fill the interval to the next note with the scale steps
///     between, inverting with the direction of motion.
/// </summary>
internal sealed partial class OrnamentationProcessorConfigurationFactory
{
    private OrnamentationProcessorConfiguration CreatePassingTone(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.PassingTone,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, interval: PassingToneInterval)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateDelayedPassingTone(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DelayedPassingTone,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, interval: 2)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateDoublePassingTone(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DoublePassingTone,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, 3)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1, 2],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0, 1 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateDelayedDoublePassingTone(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DelayedDoublePassingTone,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, 3)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1, 2],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0, 1 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateRun(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.Run,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, interval: 4)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1, 2, 3],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0, 1, 2 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateDelayedRun(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DelayedRun,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, 5)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1, 2, 3, 4],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0, 1, 2, 3 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateDoubleRun(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DoubleRun,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, 5)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1, 2, 3, 1, 2, 3, 4],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0, 1, 2, 3, 4, 5, 6 }.ToFrozenSet()
    );
}
