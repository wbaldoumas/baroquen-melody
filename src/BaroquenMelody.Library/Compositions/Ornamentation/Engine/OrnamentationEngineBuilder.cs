using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Processors;
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
        .WithProcessors(BuildOrnamentationProcessors())
        .WithOutputPolicies(new CleanConflictingOrnamentations(BuildOrnamentationCleaningEngine()))
        .Build();

    public IProcessor<OrnamentationItem> BuildSustainedNoteEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator),
            new IsRepeatedNote(),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, SustainedNoteProcessor.Interval)
        )
        .WithProcessors(new SustainedNoteProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Sustain, logger))
        .Build();

#pragma warning disable MA0051 // Method is too long
    private IProcessor<OrnamentationItem>[] BuildOrnamentationProcessors()
#pragma warning restore MA0051 // Method is too long
    {
        var processors = new List<IProcessor<OrnamentationItem>>();

        foreach (var ornamentationConfiguration in compositionConfiguration.AggregateOrnamentationConfiguration.Configurations.Where(configuration => configuration.IsEnabled))
        {
            switch (ornamentationConfiguration.OrnamentationType)
            {
                case OrnamentationType.PassingTone:
                    processors.Add(BuildPassingToneEngine(ornamentationConfiguration));
                    break;
                case OrnamentationType.Run:
                    processors.Add(BuildRunEngine(ornamentationConfiguration));
                    break;
                case OrnamentationType.DelayedPassingTone:
                    processors.Add(BuildDelayedPassingToneEngine(ornamentationConfiguration));
                    break;
                case OrnamentationType.Turn:
                    processors.Add(BuildTurnEngine(ornamentationConfiguration));
                    break;
                case OrnamentationType.AlternateTurn:
                    processors.Add(BuildAlternateTurnEngine(ornamentationConfiguration));
                    break;
                case OrnamentationType.DelayedRun:
                    processors.Add(BuildDelayedRunEngine(ornamentationConfiguration));
                    break;
                case OrnamentationType.DoubleTurn:
                    processors.Add(BuildDoubleTurnEngine(ornamentationConfiguration));
                    break;
                case OrnamentationType.DoublePassingTone:
                    processors.Add(BuildDoublePassingToneEngine(ornamentationConfiguration));
                    break;
                case OrnamentationType.DelayedDoublePassingTone:
                    processors.Add(BuildDelayedDoublePassingToneEngine(ornamentationConfiguration));
                    break;
                case OrnamentationType.DecorateInterval:
                    processors.Add(BuildDecorateDominantSeventhIntervalEngine(compositionConfiguration.Scale.Supertonic, DecorateDominantSeventhBelowSupertonicInterval, ornamentationConfiguration));
                    processors.Add(BuildDecorateDominantSeventhIntervalEngine(compositionConfiguration.Scale.Supertonic, DecorateDominantSeventhAboveSupertonicInterval, ornamentationConfiguration));
                    processors.Add(BuildDecorateDominantSeventhIntervalEngine(compositionConfiguration.Scale.LeadingTone, DecorateDominantSeventhAboveLeadingToneInterval, ornamentationConfiguration));
                    processors.Add(BuildDecorateDominantSeventhIntervalEngine(compositionConfiguration.Scale.LeadingTone, DecorateDominantSeventhBelowLeadingToneInterval, ornamentationConfiguration));
                    break;
                case OrnamentationType.DoubleRun:
                    processors.Add(BuildDoubleRunProcessor(ornamentationConfiguration));
                    break;
                case OrnamentationType.Pedal:
                    processors.Add(BuildPedalProcessor(new IsRootOfChord(_chordNumberIdentifier, compositionConfiguration), PedalProcessor.RootPedalInterval, ornamentationConfiguration));
                    processors.Add(BuildPedalProcessor(new IsThirdOfChord(_chordNumberIdentifier, compositionConfiguration), PedalProcessor.ThirdPedalInterval, ornamentationConfiguration));
                    processors.Add(BuildPedalProcessor(new IsFifthOfChord(_chordNumberIdentifier, compositionConfiguration), PedalProcessor.FifthPedalInterval, ornamentationConfiguration));
                    break;
                case OrnamentationType.Mordent:
                    processors.Add(BuildMordentProcessor(ornamentationConfiguration));
                    break;
                case OrnamentationType.RepeatedNote:
                    processors.Add(BuildRepeatedNoteProcessor(OrnamentationType.RepeatedNote, ornamentationConfiguration));
                    break;
                case OrnamentationType.DelayedRepeatedNote:
                    processors.Add(BuildRepeatedNoteProcessor(OrnamentationType.DelayedRepeatedNote, ornamentationConfiguration));
                    break;
                case OrnamentationType.NeighborTone:
                    processors.Add(BuildNeighborToneProcessor(OrnamentationType.NeighborTone, ornamentationConfiguration));
                    break;
                case OrnamentationType.DelayedNeighborTone:
                    processors.Add(BuildNeighborToneProcessor(OrnamentationType.DelayedNeighborTone, ornamentationConfiguration));
                    break;
                case OrnamentationType.Sustain:
                case OrnamentationType.MidSustain:
                case OrnamentationType.Rest:
                case OrnamentationType.None:
                default:
                    break;
            }
        }

        return [.. processors];
    }

    private IPolicyEngine<OrnamentationItem> BuildPassingToneEngine(OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, PassingToneProcessor.Interval)
        )
        .WithProcessors(new PassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.PassingTone))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.PassingTone, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDelayedPassingToneEngine(OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, PassingToneProcessor.Interval)
        )
        .WithProcessors(new PassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.DelayedPassingTone))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DelayedPassingTone, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildTurnEngine(OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, TurnProcessor.Interval)
        )
        .WithProcessors(new TurnProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Turn, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildAlternateTurnEngine(OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, AlternateTurnProcessor.Interval)
        )
        .WithProcessors(new AlternateTurnProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.AlternateTurn, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildRunEngine(OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, RunProcessor.Interval)
        )
        .WithProcessors(new RunProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Run, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDelayedRunEngine(OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, DelayedRunProcessor.Interval)
        )
        .WithProcessors(new DelayedRunProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DelayedRun, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDoubleTurnEngine(OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, DoubleTurnProcessor.Interval)
        )
        .WithProcessors(new DoubleTurnProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DoubleTurn, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDoublePassingToneEngine(OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, DoublePassingToneProcessor.Interval)
        )
        .WithProcessors(new DoublePassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.DoublePassingTone))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DoublePassingTone, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDelayedDoublePassingToneEngine(OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, DoublePassingToneProcessor.Interval)
        )
        .WithProcessors(new DoublePassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.DelayedDoublePassingTone))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DelayedDoublePassingTone, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDecorateDominantSeventhIntervalEngine(NoteName targetNote, int intervalChange, OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new HasNextBeat(),
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
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

    private IPolicyEngine<OrnamentationItem> BuildDoubleRunProcessor(OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, DoubleRunProcessor.Interval)
        )
        .WithProcessors(new DoubleRunProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DoubleRun, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildPedalProcessor(IInputPolicy<OrnamentationItem> scaleDegreePolicy, int pedalInterval, OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            scaleDegreePolicy,
            new IsApplicableInterval(compositionConfiguration, PedalProcessor.Interval),
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.Pedal)),
            new IsIntervalWithinVoiceRange(compositionConfiguration, pedalInterval)
        )
        .WithProcessors(new PedalProcessor(musicalTimeSpanCalculator, compositionConfiguration, pedalInterval))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Pedal, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildMordentProcessor(OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.Mordent)),
            new IsIntervalWithinVoiceRange(compositionConfiguration, 1).And(new IsIntervalWithinVoiceRange(compositionConfiguration, -1))
        )
        .WithProcessors(new MordentProcessor(musicalTimeSpanCalculator, _weightedRandomBooleanGenerator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Mordent, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildRepeatedNoteProcessor(OrnamentationType ornamentationType, OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation
        )
        .WithProcessors(new RepeatedNoteProcessor(musicalTimeSpanCalculator, compositionConfiguration, ornamentationType))
        .WithOutputPolicies(new LogOrnamentation(ornamentationType, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildNeighborToneProcessor(OrnamentationType ornamentationType, OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
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
