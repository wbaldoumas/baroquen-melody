﻿using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.FourFour;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.MeterAgnostic;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.ThreeFour;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Infrastructure.Random;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine;

[ExcludeFromCodeCoverage(Justification = "Trivial builder methods.")]
internal sealed class OrnamentationEngineBuilder(CompositionConfiguration compositionConfiguration, IMusicalTimeSpanCalculator musicalTimeSpanCalculator, ILogger logger)
{
    private const int DecorateDominantSeventhAboveSupertonicInterval = 3;
    private const int DecorateDominantSeventhBelowSupertonicInterval = -4;
    private const int DecorateDominantSeventhAboveLeadingToneInterval = 5;
    private const int DecorateDominantSeventhBelowLeadingToneInterval = -2;

    private readonly IChordNumberIdentifier _chordNumberIdentifier = new ChordNumberIdentifier(compositionConfiguration);

    private readonly IWeightedRandomBooleanGenerator _weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator();

    private readonly IInputPolicy<OrnamentationItem> _hasNoOrnamentation = new Not<OrnamentationItem>(new HasOrnamentation());

    private readonly FourFourOrnamentationCleaningEngineBuilder _fourFourOrnamentationCleaningEngineBuilder = new(
        new QuarterNoteOrnamentationCleaner(compositionConfiguration),
        new EighthNoteOrnamentationCleaner(compositionConfiguration),
        new QuarterEighthNoteOrnamentationCleaner(compositionConfiguration),
        new TurnAlternateTurnOrnamentationCleaner(compositionConfiguration),
        new SixteenthNoteOrnamentationCleaner(compositionConfiguration),
        new SixteenthEighthNoteOrnamentationCleaner(compositionConfiguration),
        new MordentEighthNoteOrnamentationCleaner(compositionConfiguration)
    );

    private readonly ThreeFourOrnamentationCleaningEngineBuilder _threeFourOrnamentationCleaningEngineBuilder = new(
        new HalfQuarterOrnamentationCleaner(compositionConfiguration),
        new EighthNoteOrnamentationCleaner(compositionConfiguration),
        new HalfEighthNoteOrnamentationCleaner(compositionConfiguration),
        new SixteenthEighthNoteOrnamentationCleaner(compositionConfiguration),
        new DelayedRunEighthOrnamentationCleaner(compositionConfiguration),
        new DoublePassingToneQuarterOrnamentationCleaner(compositionConfiguration),
        new DoublePassingToneOrnamentationCleaner(compositionConfiguration),
        new HalfQuarterEighthOrnamentationCleaner(compositionConfiguration),
        new QuarterQuarterEighthOrnamentationCleaner(compositionConfiguration),
        new DoublePassingToneDelayedRunOrnamentationCleaner(compositionConfiguration)
    );

    public IPolicyEngine<OrnamentationItem> BuildOrnamentationEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithoutInputPolicies()
        .WithProcessors(
            BuildPassingToneEngine(),
            BuildDoublePassingToneEngine(),
            BuildDelayedDoublePassingToneEngine(),
            BuildDoubleTurnEngine(),
            BuildDelayedPassingToneEngine(),
            BuildNeighborToneProcessor(OrnamentationType.DelayedNeighborTone),
            BuildNeighborToneProcessor(OrnamentationType.NeighborTone),
            BuildRunEngine(),
            BuildDoubleRunProcessor(),
            BuildTurnEngine(),
            BuildAlternateTurnEngine(),
            BuildDelayedRunEngine(),
            BuildMordentProcessor(),
            BuildDecorateDominantSeventhIntervalEngine(compositionConfiguration.Scale.Supertonic, DecorateDominantSeventhBelowSupertonicInterval),
            BuildDecorateDominantSeventhIntervalEngine(compositionConfiguration.Scale.Supertonic, DecorateDominantSeventhAboveSupertonicInterval),
            BuildDecorateDominantSeventhIntervalEngine(compositionConfiguration.Scale.LeadingTone, DecorateDominantSeventhAboveLeadingToneInterval),
            BuildDecorateDominantSeventhIntervalEngine(compositionConfiguration.Scale.LeadingTone, DecorateDominantSeventhBelowLeadingToneInterval),
            BuildPedalProcessor(new IsRootOfChord(_chordNumberIdentifier, compositionConfiguration), PedalProcessor.RootPedalInterval),
            BuildPedalProcessor(new IsThirdOfChord(_chordNumberIdentifier, compositionConfiguration), PedalProcessor.ThirdPedalInterval),
            BuildPedalProcessor(new IsFifthOfChord(_chordNumberIdentifier, compositionConfiguration), PedalProcessor.FifthPedalInterval),
            BuildRepeatedNoteProcessor(OrnamentationType.RepeatedNote),
            BuildRepeatedNoteProcessor(OrnamentationType.DelayedRepeatedNote)
        )
        .WithOutputPolicies(new CleanConflictingOrnamentations(BuildOrnamentationCleaningEngine()))
        .Build();

    public IPolicyEngine<OrnamentationItem> BuildSustainedNoteEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator),
            new IsRepeatedNote(),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, SustainedNoteProcessor.Interval)
        )
        .WithProcessors(new SustainedNoteProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Sustain, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildPassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, PassingToneProcessor.Interval)
        )
        .WithProcessors(new PassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.PassingTone))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.PassingTone, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDelayedPassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, PassingToneProcessor.Interval)
        )
        .WithProcessors(new PassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.DelayedPassingTone))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DelayedPassingTone, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildTurnEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, TurnProcessor.Interval)
        )
        .WithProcessors(new TurnProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Turn, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildAlternateTurnEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, AlternateTurnProcessor.Interval)
        )
        .WithProcessors(new AlternateTurnProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.AlternateTurn, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildRunEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, RunProcessor.Interval)
        )
        .WithProcessors(new RunProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Run, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDelayedRunEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, 15),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, DelayedRunProcessor.Interval)
        )
        .WithProcessors(new DelayedRunProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DelayedRun, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDoubleTurnEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, 30),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, DoubleTurnProcessor.Interval)
        )
        .WithProcessors(new DoubleTurnProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DoubleTurn, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDoublePassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, DoublePassingToneProcessor.Interval)
        )
        .WithProcessors(new DoublePassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.DoublePassingTone))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DoublePassingTone, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDelayedDoublePassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, DoublePassingToneProcessor.Interval)
        )
        .WithProcessors(new DoublePassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.DelayedDoublePassingTone))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DelayedDoublePassingTone, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDecorateDominantSeventhIntervalEngine(NoteName targetNote, int intervalChange) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new HasNextBeat(),
            new WantsToOrnament(_weightedRandomBooleanGenerator, 30),
            _hasNoOrnamentation,
            new IsTargetNote(targetNote),
            new HasTargetNotes([compositionConfiguration.Scale.Dominant, compositionConfiguration.Scale.LeadingTone, compositionConfiguration.Scale.Supertonic]),
            new NextBeatHasTargetNotes([compositionConfiguration.Scale.Tonic, compositionConfiguration.Scale.Mediant, compositionConfiguration.Scale.Dominant]),
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.DecorateInterval)),
            new IsIntervalWithinVoiceRange(compositionConfiguration, intervalChange)
        )
        .WithProcessors(new DecorateIntervalProcessor(musicalTimeSpanCalculator, compositionConfiguration, intervalChange))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DecorateInterval, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDoubleRunProcessor() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, 15),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, DoubleRunProcessor.Interval)
        )
        .WithProcessors(new DoubleRunProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DoubleRun, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildPedalProcessor(IInputPolicy<OrnamentationItem> scaleDegreePolicy, int pedalInterval) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator),
            _hasNoOrnamentation,
            scaleDegreePolicy,
            new IsApplicableInterval(compositionConfiguration, PedalProcessor.Interval),
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.Pedal)),
            new IsIntervalWithinVoiceRange(compositionConfiguration, pedalInterval)
        )
        .WithProcessors(new PedalProcessor(musicalTimeSpanCalculator, compositionConfiguration, pedalInterval))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Pedal, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildMordentProcessor() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, 1),
            _hasNoOrnamentation,
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.Mordent)),
            new IsIntervalWithinVoiceRange(compositionConfiguration, 1).And(new IsIntervalWithinVoiceRange(compositionConfiguration, -1))
        )
        .WithProcessors(new MordentProcessor(musicalTimeSpanCalculator, _weightedRandomBooleanGenerator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Mordent, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildRepeatedNoteProcessor(OrnamentationType ornamentationType) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, 5),
            _hasNoOrnamentation
        )
        .WithProcessors(new RepeatedNoteProcessor(musicalTimeSpanCalculator, compositionConfiguration, ornamentationType))
        .WithOutputPolicies(new LogOrnamentation(ornamentationType, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildNeighborToneProcessor(OrnamentationType ornamentationType) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, 25),
            _hasNoOrnamentation,
            new IsRepeatedNote()
        )
        .WithProcessors(new NeighborToneProcessor(musicalTimeSpanCalculator, _weightedRandomBooleanGenerator, ornamentationType, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DelayedNeighborTone, logger))
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildOrnamentationCleaningEngine() => compositionConfiguration.Meter == Meter.FourFour
        ? _fourFourOrnamentationCleaningEngineBuilder.Build()
        : _threeFourOrnamentationCleaningEngineBuilder.Build();
}
