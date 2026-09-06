using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

/// <summary>
///     The pickups: figures that approach the NEXT note from a step or more away, so their translations are
///     applied to the next note rather than the current one (<c>ShouldTranslateOnCurrentNote: false</c>) and their
///     range guards look at both notes.
/// </summary>
internal sealed partial class OrnamentationProcessorConfigurationFactory
{
    private OrnamentationProcessorConfiguration CreatePickup(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.Pickup,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            _hasNextBeat,
            _isNotRepeatedNote,
            new IsNextNoteIntervalWithinInstrumentRange(_compositionConfiguration, 1).And(new IsIntervalWithinInstrumentRange(_compositionConfiguration, -1))
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet(),
        ShouldTranslateOnCurrentNote: false
    );

    private OrnamentationProcessorConfiguration CreateDelayedPickup(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DelayedPickup,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            _hasNextBeat,
            _isNotRepeatedNote,
            new IsNextNoteIntervalWithinInstrumentRange(_compositionConfiguration, 1).And(new IsIntervalWithinInstrumentRange(_compositionConfiguration, -1))
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [1],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet(),
        ShouldTranslateOnCurrentNote: false
    );

    private OrnamentationProcessorConfiguration CreateDoublePickup(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DoublePickup,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            _hasNextBeat,
            _isNotRepeatedNote,
            new IsNextNoteIntervalWithinInstrumentRange(_compositionConfiguration, 2).And(new IsIntervalWithinInstrumentRange(_compositionConfiguration, -2))
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [2, 1],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0, 1 }.ToFrozenSet(),
        ShouldTranslateOnCurrentNote: false
    );

    private OrnamentationProcessorConfiguration CreateDelayedDoublePickup(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DelayedDoublePickup,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            _hasNextBeat,
            _isNotRepeatedNote,
            new IsNextNoteIntervalWithinInstrumentRange(_compositionConfiguration, 2).And(new IsIntervalWithinInstrumentRange(_compositionConfiguration, -2))
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [2, 1],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0, 1 }.ToFrozenSet(),
        ShouldTranslateOnCurrentNote: false
    );

    private OrnamentationProcessorConfiguration CreateTriplePickup(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.TriplePickup,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            _hasNextBeat,
            _isNotRepeatedNote,
            new IsNextNoteIntervalWithinInstrumentRange(_compositionConfiguration, 3).And(new IsIntervalWithinInstrumentRange(_compositionConfiguration, -3))
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [3, 2, 1],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0, 1, 2 }.ToFrozenSet(),
        ShouldTranslateOnCurrentNote: false
    );
}
