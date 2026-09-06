using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

/// <summary>
///     The harmonic decorations: figures gated on what the current and next harmonies contain rather than on the
///     melodic interval alone — the dominant-seventh decorations of the supertonic and leading tone, the
///     decorated third, and the sequenced thirds.
/// </summary>
internal sealed partial class OrnamentationProcessorConfigurationFactory
{
    private const int DecorateDominantSeventhAboveSupertonicInterval = 3;

    private const int DecorateDominantSeventhBelowSupertonicInterval = -4;

    private const int DecorateDominantSeventhAboveLeadingToneInterval = 5;

    private const int DecorateDominantSeventhBelowLeadingToneInterval = -2;

    private IEnumerable<OrnamentationProcessorConfiguration> CreateDecorateIntervals(OrnamentationConfiguration configuration) =>
    [
        CreateDecorateInterval(_compositionConfiguration.Scale.Supertonic, DecorateDominantSeventhBelowSupertonicInterval, configuration),
        CreateDecorateInterval(_compositionConfiguration.Scale.Supertonic, DecorateDominantSeventhAboveSupertonicInterval, configuration),
        CreateDecorateInterval(_compositionConfiguration.Scale.LeadingTone, DecorateDominantSeventhAboveLeadingToneInterval, configuration),
        CreateDecorateInterval(_compositionConfiguration.Scale.LeadingTone, DecorateDominantSeventhBelowLeadingToneInterval, configuration)
    ];

    private OrnamentationProcessorConfiguration CreateDecorateInterval(NoteName targetNote, int intervalChange, OrnamentationConfiguration configuration) => new(
        OrnamentationType.DecorateInterval,
        InputPolicies:
        [
            _hasNextBeat,
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsTargetNote(targetNote),
            new HasTargetNotes([_compositionConfiguration.Scale.Dominant, _compositionConfiguration.Scale.LeadingTone, _compositionConfiguration.Scale.Supertonic]),
            new NextBeatHasTargetNotes([_compositionConfiguration.Scale.Tonic, _compositionConfiguration.Scale.Mediant, _compositionConfiguration.Scale.Dominant]),
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.DecorateInterval)),
            new IsIntervalWithinInstrumentRange(_compositionConfiguration, intervalChange)
        ],
        OutputPolicies: [new LogOrnamentation(configuration.OrnamentationType, _logger)],
        Translations: [intervalChange, intervalChange - 1, intervalChange],
        ShouldNotInvert,
        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateDecorateThird(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.DecorateThird,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            _hasNextBeat,
            _isDescending.And(new IsApplicableInterval(_compositionConfiguration, interval: 1)).Or(_isRepeatedNote)
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [-1, 0, -2],
        ShouldNotInvert,
        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreateSequencedThirds(IInputPolicy<OrnamentationItem> wantsToOrnament, IOutputPolicy<OrnamentationItem> logOrnamentation) => new(
        OrnamentationType.SequencedThirds,
        InputPolicies:
        [
            wantsToOrnament,
            _hasNoOrnamentation,
            _hasNextBeat,
            _isNotRepeatedNote,
            new IsApplicableInterval(_compositionConfiguration, 4),
            _isAscending.And(new IsNextNoteIntervalWithinInstrumentRange(_compositionConfiguration, 1))
                .Or(_isDescending.And(new IsNextNoteIntervalWithinInstrumentRange(_compositionConfiguration, -1)))
        ],
        OutputPolicies: [logOrnamentation],
        Translations: [2, 1, 3, 2, 4, 3, 5],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 0, 1, 2, 3, 4, 5, 6 }.ToFrozenSet(),
        ShouldTranslateOnCurrentNote: true
    );
}
