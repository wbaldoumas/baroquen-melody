using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

/// <summary>
///     The figures that stay on the principal or its neighbours: repetitions, neighbour tones, the mordent and
///     trill that shake against the upper neighbour, and the appoggiatura that leans on it.
/// </summary>
internal sealed partial class OrnamentationProcessorConfigurationFactory
{
    private static OrnamentationProcessorConfiguration CreateRepeatedNote(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.RepeatedNote,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [0],
        ShouldNotInvert,
        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
    );

    private static OrnamentationProcessorConfiguration CreateDelayedRepeatedNote(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DelayedRepeatedNote,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [0],
        ShouldNotInvert,
        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateNeighborTone(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.NeighborTone,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            _isRepeatedNote
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1],
        ShouldInvertRandomly,
        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateDelayedNeighborTone(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DelayedNeighborTone,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            _isRepeatedNote
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1],
        ShouldInvertRandomly,
        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateMordent(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.Mordent,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.Mordent)),
            new HasNeighborNotesWithinInstrumentRange(_compositionConfiguration)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1, 0],
        ShouldInvertRandomly,
        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateTrill(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.Trill,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.Trill)),
            new HasNeighborNotesWithinInstrumentRange(_compositionConfiguration)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1, 0, 1, 0, 1, -1, 0],
        ShouldNotInvert,
        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateAppoggiatura(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.Appoggiatura,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.Appoggiatura)),
            new IsIntervalWithinInstrumentRange(_compositionConfiguration, 1),
            new LeaningToneIsDissonant(_compositionConfiguration),
            new LeaningToneIsNotRestruck(_compositionConfiguration)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1, 0],
        ShouldNotInvert,
        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
    );
}
