using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

/// <summary>
///     The turns: figures that circle the principal through its upper and lower neighbours before moving on, plain
///     or inverted, single or doubled across the interval.
/// </summary>
internal sealed partial class OrnamentationProcessorConfigurationFactory
{
    private OrnamentationProcessorConfiguration CreateTurn(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.Turn,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, interval: 2)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [-1, 0, 1],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0, 1, 2 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateInvertedTurn(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.InvertedTurn,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, interval: 1)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1, -1, 0],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0, 1, 2 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateDoubleTurn(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DoubleTurn,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, interval: 4)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [-1, 0, 1, 2, 1, 2, 3],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0, 1, 2, 3, 4, 5, 6 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateDoubleInvertedTurn(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DoubleInvertedTurn,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, interval: 2)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1, -1, 0, 1, 2, 0, 1],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0, 1, 2, 3, 4, 5, 6 }.ToFrozenSet()
    );
}
