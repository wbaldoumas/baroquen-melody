using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Processors;
using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation.Cleaning;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Configuration;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection.Strategies;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using LazyCart;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Ornamentation.Engine;

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

    private readonly NoteIndexPairSelector _noteIndexPairSelector = new(new NoteOnsetCalculator(musicalTimeSpanCalculator, compositionConfiguration));

    public IPolicyEngine<OrnamentationItem> BuildOrnamentationEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithoutInputPolicies()
        .WithProcessors(BuildOrnamentationProcessors())
        .WithOutputPolicies(new CleanConflictingOrnamentations(BuildGeneralizedOrnamentationCleaningEngine()))
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
                case OrnamentationType.InvertedTurn:
                    processors.Add(BuildInvertedTurnEngine(ornamentationConfiguration));
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
                case OrnamentationType.Pickup:
                    processors.Add(BuildPickupProcessor(OrnamentationType.Pickup, ornamentationConfiguration));
                    break;
                case OrnamentationType.DelayedPickup:
                    processors.Add(BuildPickupProcessor(OrnamentationType.DelayedPickup, ornamentationConfiguration));
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

    private IPolicyEngine<OrnamentationItem> BuildInvertedTurnEngine(OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsApplicableInterval(compositionConfiguration, InvertedTurnProcessor.Interval)
        )
        .WithProcessors(new InvertedTurnProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.InvertedTurn, logger))
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

    private IPolicyEngine<OrnamentationItem> BuildDecorateDominantSeventhIntervalEngine(NoteName targetNote, int intervalChange, OrnamentationConfiguration configuration) =>
        PolicyEngineBuilder<OrnamentationItem>.Configure()
            .WithInputPolicies(
                new HasNextBeat(),
                new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
                _hasNoOrnamentation,
                new IsTargetNote(targetNote),
                new HasTargetNotes([compositionConfiguration.Scale.Dominant, compositionConfiguration.Scale.LeadingTone, compositionConfiguration.Scale.Supertonic]),
                new NextBeatHasTargetNotes([compositionConfiguration.Scale.Tonic, compositionConfiguration.Scale.Mediant, compositionConfiguration.Scale.Dominant]),
                new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.DecorateInterval)),
                new IsIntervalWithinInstrumentRange(compositionConfiguration, intervalChange)
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

    private IPolicyEngine<OrnamentationItem> BuildPedalProcessor(IInputPolicy<OrnamentationItem> scaleDegreePolicy, int pedalInterval, OrnamentationConfiguration configuration) =>
        PolicyEngineBuilder<OrnamentationItem>.Configure()
            .WithInputPolicies(
                new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
                _hasNoOrnamentation,
                scaleDegreePolicy,
                new IsApplicableInterval(compositionConfiguration, PedalProcessor.Interval),
                new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.Pedal)),
                new IsIntervalWithinInstrumentRange(compositionConfiguration, pedalInterval)
            )
            .WithProcessors(new PedalProcessor(musicalTimeSpanCalculator, compositionConfiguration, pedalInterval))
            .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Pedal, logger))
            .Build();

    private IPolicyEngine<OrnamentationItem> BuildMordentProcessor(OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.Mordent)),
            new IsIntervalWithinInstrumentRange(compositionConfiguration, 1).And(new IsIntervalWithinInstrumentRange(compositionConfiguration, -1))
        )
        .WithProcessors(new MordentProcessor(musicalTimeSpanCalculator, _weightedRandomBooleanGenerator, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Mordent, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildRepeatedNoteProcessor(OrnamentationType ornamentationType, OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>
        .Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation
        )
        .WithProcessors(new RepeatedNoteProcessor(musicalTimeSpanCalculator, compositionConfiguration, ornamentationType))
        .WithOutputPolicies(new LogOrnamentation(ornamentationType, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildNeighborToneProcessor(OrnamentationType ornamentationType, OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>
        .Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new IsRepeatedNote()
        )
        .WithProcessors(new NeighborToneProcessor(musicalTimeSpanCalculator, _weightedRandomBooleanGenerator, ornamentationType, compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.DelayedNeighborTone, logger))
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildPickupProcessor(OrnamentationType ornamentationType, OrnamentationConfiguration configuration) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            new HasNextBeat(),
            new Not<OrnamentationItem>(new IsRepeatedNote()),
            new IsNextNoteIntervalWithinInstrumentRange(compositionConfiguration, 1).And(new IsNextNoteIntervalWithinInstrumentRange(compositionConfiguration, -1))
        )
        .WithProcessors(new PickupProcessor(musicalTimeSpanCalculator, compositionConfiguration, ornamentationType))
        .WithOutputPolicies(new LogOrnamentation(ornamentationType, logger))
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildGeneralizedOrnamentationCleaningEngine()
    {
        var ornamentationTypes = EnumUtils<OrnamentationType>
            .AsEnumerable()
            .Where(ornamentationType => ornamentationType is not OrnamentationType.None
                and not OrnamentationType.Sustain
                and not OrnamentationType.MidSustain
                and not OrnamentationType.Rest
            )
            .ToList();

        var cleaningSelector = new NoteTargetSelector(
            new List<IOrnamentationCleaningSelectorStrategy>
            {
                new CleanTargetOrnamentation(),
                new CleanLowerNote(),
                new CleanRandomNote(_weightedRandomBooleanGenerator)
            }
        );

        var ornamentationCombinations = new LazyCartesianProduct<OrnamentationType, OrnamentationType>(ornamentationTypes, ornamentationTypes);

        var processors = new List<IProcessor<OrnamentationCleaningItem>>();

        for (var i = 0; i < ornamentationCombinations.Size; i++)
        {
            var (primaryOrnamentation, secondaryOrnamentation) = ornamentationCombinations[i];

            var noteSelector = new NotePairSelector(primaryOrnamentation, secondaryOrnamentation);
            var indices = _noteIndexPairSelector.Select(primaryOrnamentation, secondaryOrnamentation);

            var ornamentationCleaningConfiguration = new OrnamentationCleanerConfiguration(
                noteSelector,
                indices,
                cleaningSelector
            );

            IProcessor<OrnamentationCleaningItem> processor = PolicyEngineBuilder<OrnamentationCleaningItem>
                .Configure()
                .WithInputPolicies(new HasTargetOrnamentations(primaryOrnamentation, secondaryOrnamentation))
                .WithProcessors(new OrnamentationCleaner(ornamentationCleaningConfiguration, compositionConfiguration, _weightedRandomBooleanGenerator))
                .Build();

            processors.Add(processor);
        }

        return PolicyEngineBuilder<OrnamentationCleaningItem>
            .Configure()
            .WithoutInputPolicies()
            .WithProcessors(processors.ToArray())
            .WithOutputPolicies()
            .Build();
    }
}
