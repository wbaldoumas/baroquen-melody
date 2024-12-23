using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

internal sealed class OrnamentationProcessorConfigurationFactory(
    IChordNumberIdentifier chordNumberIdentifier,
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    CompositionConfiguration compositionConfiguration,
    ILogger logger
) : IOrnamentationProcessorConfigurationFactory
{
    private const int DecorateDominantSeventhAboveSupertonicInterval = 3;

    private const int DecorateDominantSeventhBelowSupertonicInterval = -4;

    private const int DecorateDominantSeventhAboveLeadingToneInterval = 5;

    private const int DecorateDominantSeventhBelowLeadingToneInterval = -2;

    private const int PassingToneInterval = 2;

    private const int RootPedalInterval = -3;

    private const int ThirdPedalInterval = -2;

    private const int FifthPedalInterval = -4;

    private static readonly IInputPolicy<OrnamentationItem> _hasNoOrnamentation = new Not<OrnamentationItem>(new HasOrnamentation());

    private static readonly IInputPolicy<OrnamentationItem> _hasNextBeat = new HasNextBeat();

    private static readonly IInputPolicy<OrnamentationItem> _isRepeatedNote = new IsRepeatedNote();

    private static readonly IInputPolicy<OrnamentationItem> _isAscending = new IsAscending();

    private static readonly IInputPolicy<OrnamentationItem> _isDescending = new IsDescending();

    private static readonly IInputPolicy<OrnamentationItem> _isNotRepeatedNote = new Not<OrnamentationItem>(new IsRepeatedNote());

    private readonly IInputPolicy<OrnamentationItem> _isRootOfChord = new IsRootOfChord(chordNumberIdentifier, compositionConfiguration);

    private readonly IInputPolicy<OrnamentationItem> _isThirdOfChord = new IsThirdOfChord(chordNumberIdentifier, compositionConfiguration);

    private readonly IInputPolicy<OrnamentationItem> _isFifthOfChord = new IsFifthOfChord(chordNumberIdentifier, compositionConfiguration);

    private static readonly Predicate<(BaroquenNote? CurrentNote, BaroquenNote? NextNote)> ShouldInvertBasedOnDirection =
        notes => notes.CurrentNote is not null
                 && notes.NextNote is not null
                 && notes.CurrentNote.NoteNumber > notes.NextNote.NoteNumber;

    private static readonly Predicate<(BaroquenNote? CurrentNote, BaroquenNote? NextNote)> ShouldNotInvert = _ => false;

    private readonly Predicate<(BaroquenNote? CurrentNote, BaroquenNote? NextNote)> _shouldInvertRandomly = _ => weightedRandomBooleanGenerator.IsTrue();

    public IEnumerable<OrnamentationProcessorConfiguration> Create(OrnamentationConfiguration ornamentationConfiguration)
    {
        var processorConfigurations = new List<OrnamentationProcessorConfiguration>();
        var wantsToOrnament = new WantsToOrnament(weightedRandomBooleanGenerator, ornamentationConfiguration.Probability);
        var logOrnamentation = new LogOrnamentation(ornamentationConfiguration.OrnamentationType, logger);

        switch (ornamentationConfiguration.OrnamentationType)
        {
            case OrnamentationType.PassingTone:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.PassingTone,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsApplicableInterval(compositionConfiguration, interval: PassingToneInterval)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.Run:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.Run,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsApplicableInterval(compositionConfiguration, interval: 4)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1, 2, 3],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0, 1, 2 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.DelayedPassingTone:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.DelayedPassingTone,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsApplicableInterval(compositionConfiguration, interval: 2)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.Turn:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.Turn,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsApplicableInterval(compositionConfiguration, interval: 2)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [-1, 0, 1],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0, 1, 2 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.InvertedTurn:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.InvertedTurn,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsApplicableInterval(compositionConfiguration, interval: 1)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1, -1, 0],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0, 1, 2 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.DelayedRun:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.DelayedRun,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsApplicableInterval(compositionConfiguration, 5)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1, 2, 3, 4],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0, 1, 2, 3 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.DoubleTurn:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.DoubleTurn,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsApplicableInterval(compositionConfiguration, interval: 4)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [-1, 0, 1, 2, 1, 2, 3],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0, 1, 2, 3, 4, 5, 6 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.DoubleInvertedTurn:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.DoubleInvertedTurn,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsApplicableInterval(compositionConfiguration, interval: 2)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1, -1, 0, 1, 2, 0, 1],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0, 1, 2, 3, 4, 5, 6 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.DoublePassingTone:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.DoublePassingTone,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsApplicableInterval(compositionConfiguration, 3)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1, 2],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0, 1 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.DecorateInterval:
                processorConfigurations.Add(
                    CreateDecorateIntervalOrnamentationProcessorConfiguration(
                        compositionConfiguration.Scale.Supertonic,
                        DecorateDominantSeventhBelowSupertonicInterval,
                        ornamentationConfiguration
                    )
                );

                processorConfigurations.Add(
                    CreateDecorateIntervalOrnamentationProcessorConfiguration(
                        compositionConfiguration.Scale.Supertonic,
                        DecorateDominantSeventhAboveSupertonicInterval,
                        ornamentationConfiguration
                    )
                );

                processorConfigurations.Add(
                    CreateDecorateIntervalOrnamentationProcessorConfiguration(
                        compositionConfiguration.Scale.LeadingTone,
                        DecorateDominantSeventhAboveLeadingToneInterval,
                        ornamentationConfiguration
                    )
                );

                processorConfigurations.Add(
                    CreateDecorateIntervalOrnamentationProcessorConfiguration(
                        compositionConfiguration.Scale.LeadingTone,
                        DecorateDominantSeventhBelowLeadingToneInterval,
                        ornamentationConfiguration
                    )
                );
                break;
            case OrnamentationType.DoubleRun:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.DoubleRun,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsApplicableInterval(compositionConfiguration, 5)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1, 2, 3, 1, 2, 3, 4],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0, 1, 2, 3, 4, 5, 6 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.Pedal:
                processorConfigurations.Add(
                    CreatePedalOrnamentationProcessorConfiguration(
                        _isRootOfChord,
                        RootPedalInterval,
                        ornamentationConfiguration
                    )
                );

                processorConfigurations.Add(
                    CreatePedalOrnamentationProcessorConfiguration(
                        _isThirdOfChord,
                        ThirdPedalInterval,
                        ornamentationConfiguration
                    )
                );

                processorConfigurations.Add(
                    CreatePedalOrnamentationProcessorConfiguration(
                        _isFifthOfChord,
                        FifthPedalInterval,
                        ornamentationConfiguration
                    )
                );
                break;
            case OrnamentationType.Mordent:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.Mordent,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.Mordent)),
                            new IsIntervalWithinInstrumentRange(compositionConfiguration, 1).And(new IsIntervalWithinInstrumentRange(compositionConfiguration, -1))
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1, 0],
                        _shouldInvertRandomly,
                        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.RepeatedNote:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
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
                    )
                );
                break;
            case OrnamentationType.DelayedRepeatedNote:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
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
                    )
                );
                break;
            case OrnamentationType.NeighborTone:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.NeighborTone,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            _isRepeatedNote
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1],
                        _shouldInvertRandomly,
                        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.DelayedNeighborTone:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.DelayedNeighborTone,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            _isRepeatedNote
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1],
                        _shouldInvertRandomly,
                        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.Pickup:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.Pickup,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            _hasNextBeat,
                            _isNotRepeatedNote,
                            new IsNextNoteIntervalWithinInstrumentRange(compositionConfiguration, 1).And(new IsIntervalWithinInstrumentRange(compositionConfiguration, -1))
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet(),
                        ShouldTranslateOnCurrentNote: false
                    )
                );
                break;
            case OrnamentationType.DelayedDoublePassingTone:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.DelayedDoublePassingTone,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsApplicableInterval(compositionConfiguration, 3)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1, 2],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0, 1 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.DelayedPickup:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.DelayedPickup,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            _hasNextBeat,
                            _isNotRepeatedNote,
                            new IsNextNoteIntervalWithinInstrumentRange(compositionConfiguration, 1).And(new IsIntervalWithinInstrumentRange(compositionConfiguration, -1))
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [1],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0 }.ToFrozenSet(),
                        ShouldTranslateOnCurrentNote: false
                    )
                );
                break;
            case OrnamentationType.DoublePickup:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.DoublePickup,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            _hasNextBeat,
                            _isNotRepeatedNote,
                            new IsNextNoteIntervalWithinInstrumentRange(compositionConfiguration, 2).And(new IsIntervalWithinInstrumentRange(compositionConfiguration, -2))
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [2, 1],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0, 1 }.ToFrozenSet(),
                        ShouldTranslateOnCurrentNote: false
                    )
                );
                break;
            case OrnamentationType.DelayedDoublePickup:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.DelayedDoublePickup,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            _hasNextBeat,
                            _isNotRepeatedNote,
                            new IsNextNoteIntervalWithinInstrumentRange(compositionConfiguration, 2).And(new IsIntervalWithinInstrumentRange(compositionConfiguration, -2))
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [2, 1],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 0, 1 }.ToFrozenSet(),
                        ShouldTranslateOnCurrentNote: false
                    )
                );
                break;
            case OrnamentationType.DecorateThird:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.DecorateThird,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            _hasNextBeat,
                            _isDescending.And(new IsApplicableInterval(compositionConfiguration, interval: 1)).Or(_isRepeatedNote)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [-1, 0, -2],
                        ShouldNotInvert,
                        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.OctavePedal:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.OctavePedal,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsIntervalWithinInstrumentRange(compositionConfiguration, -7),
                            new Not<OrnamentationItem>(new IsApplicableInterval(compositionConfiguration, PassingToneInterval)),
                            new Not<OrnamentationItem>(new IsApplicableInterval(compositionConfiguration, 4))
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [-7, 0, -7],
                        ShouldNotInvert,
                        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.OctavePedalPassingTone:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.OctavePedalPassingTone,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsIntervalWithinInstrumentRange(compositionConfiguration, -7),
                            new IsApplicableInterval(compositionConfiguration, PassingToneInterval).Or(_isRepeatedNote)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [-7, 1, -7],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 1 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.OctavePedalArpeggio:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.OctavePedalPassingTone,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            _hasNextBeat,
                            _isAscending,
                            new IsIntervalWithinInstrumentRange(compositionConfiguration, -7),
                            new IsApplicableInterval(compositionConfiguration, 4).Or(_isRepeatedNote),
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [-7, 2, -7],
                        ShouldNotInvert,
                        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.UpperOctavePedal:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.UpperOctavePedal,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsIntervalWithinInstrumentRange(compositionConfiguration, 7),
                            new Not<OrnamentationItem>(new IsApplicableInterval(compositionConfiguration, PassingToneInterval)),
                            new Not<OrnamentationItem>(new IsApplicableInterval(compositionConfiguration, 4))
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [7, 0, 7],
                        ShouldNotInvert,
                        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.UpperOctavePedalPassingTone:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.UpperOctavePedalPassingTone,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            new IsIntervalWithinInstrumentRange(compositionConfiguration, 7),
                            new IsApplicableInterval(compositionConfiguration, PassingToneInterval).Or(_isRepeatedNote)
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [7, 1, 7],
                        ShouldInvertBasedOnDirection,
                        TranslationInversionIndices: new HashSet<int> { 1 }.ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.UpperOctavePedalArpeggio:
                processorConfigurations.Add(
                    new OrnamentationProcessorConfiguration(
                        OrnamentationType.UpperOctavePedalArpeggio,
                        InputPolicies:
                        [
                            wantsToOrnament,
                            _hasNoOrnamentation,
                            _hasNextBeat,
                            _isDescending,
                            new IsIntervalWithinInstrumentRange(compositionConfiguration, 7),
                            new IsApplicableInterval(compositionConfiguration, 4).Or(_isRepeatedNote),
                        ],
                        OutputPolicies: [logOrnamentation],
                        Translations: [7, 2, 7],
                        ShouldNotInvert,
                        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
                    )
                );
                break;
            case OrnamentationType.None:
            case OrnamentationType.Sustain:
            case OrnamentationType.MidSustain:
            case OrnamentationType.Rest:
            default:
                throw new ArgumentOutOfRangeException(nameof(ornamentationConfiguration), ornamentationConfiguration.OrnamentationType, "Ornamentation type not supported.");
        }

        return processorConfigurations;
    }

    private OrnamentationProcessorConfiguration CreateDecorateIntervalOrnamentationProcessorConfiguration(NoteName targetNote, int intervalChange, OrnamentationConfiguration configuration) => new(
        OrnamentationType.DecorateInterval,
        InputPolicies:
        [
            _hasNextBeat,
            new WantsToOrnament(weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsTargetNote(targetNote),
            new HasTargetNotes([compositionConfiguration.Scale.Dominant, compositionConfiguration.Scale.LeadingTone, compositionConfiguration.Scale.Supertonic]),
            new NextBeatHasTargetNotes([compositionConfiguration.Scale.Tonic, compositionConfiguration.Scale.Mediant, compositionConfiguration.Scale.Dominant]),
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.DecorateInterval)),
            new IsIntervalWithinInstrumentRange(compositionConfiguration, intervalChange)
        ],
        OutputPolicies: [new LogOrnamentation(configuration.OrnamentationType, logger)],
        Translations: [intervalChange, intervalChange - 1, intervalChange],
        ShouldNotInvert,
        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
    );

    private OrnamentationProcessorConfiguration CreatePedalOrnamentationProcessorConfiguration(
        IInputPolicy<OrnamentationItem> scaleDegreePolicy,
        int pedalInterval,
        OrnamentationConfiguration configuration
    ) => new(
        OrnamentationType.Pedal,
        InputPolicies:
        [
            new WantsToOrnament(weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            scaleDegreePolicy,
            new IsApplicableInterval(compositionConfiguration, interval: 2),
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.Pedal)),
            new IsIntervalWithinInstrumentRange(compositionConfiguration, pedalInterval)
        ],
        OutputPolicies: [new LogOrnamentation(configuration.OrnamentationType, logger)],
        Translations: [pedalInterval, 1, pedalInterval],
        ShouldInvertBasedOnDirection,
        TranslationInversionIndices: new HashSet<int> { 1 }.ToFrozenSet()
    );
}
