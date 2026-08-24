using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Dynamics.Engine.Builders;
using BaroquenMelody.Library.Motifs;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Phrasing;
using BaroquenMelody.Library.Rhythm;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Scoring;
using BaroquenMelody.Library.Strategies;
using Fluxor;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BaroquenMelody.Library.Tests.TestData;

/// <summary>
///     The full fugal component graph, hand-wired following the configurator's recipe (modulo the
///     spacing-satisfiability resolution, which the test geometry never triggers), exposing the
///     composition object and the ledger that <see cref="BaroquenMelodyComposerConfigurator"/> keeps
///     internal - plus a scheduler override seam for the all-standard degeneration anchor. Compared
///     graphs in byte-identity anchors share this wiring, so any drift from the configurator biases
///     neither side.
/// </summary>
internal sealed record ComposerGraph(Composer Composer, VoiceRhythmLedger Ledger, CompositionConfiguration Configuration)
{
    public static ComposerGraph Create(CompositionConfiguration configuration, int seed, IVoiceRhythmScheduler? voiceRhythmSchedulerOverride = null)
    {
        var randomProvider = new SeededRandomProvider(seed);
        var weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator(randomProvider);
        var logger = Substitute.For<ILogger<MidiFileComposition>>();
        var dispatcher = Substitute.For<IDispatcher>();
        var musicalTimeSpanCalculator = new MusicalTimeSpanCalculator();
        var voiceSpacingSatisfiabilityAnalyzer = new VoiceSpacingSatisfiabilityAnalyzer();

        var chordNumberIdentifier = new ChordNumberIdentifier(configuration);
        var chordInversionIdentifier = new ChordInversionIdentifier(chordNumberIdentifier, configuration);
        var compositionRuleFactory = new CompositionRuleFactory(configuration, weightedRandomBooleanGenerator, chordNumberIdentifier);
        var compositionRule = compositionRuleFactory.CreateAggregate(configuration.AggregateCompositionRuleConfiguration);
        var compositionStrategy = new CompositionStrategyFactory(new NoteChoiceGenerator(), compositionRule, randomProvider, logger).Create(configuration);

        var fugalEntryRuleConfiguration = new AggregateCompositionRuleConfiguration(
            configuration.AggregateCompositionRuleConfiguration.Configurations
                .Where(static ruleConfiguration => ruleConfiguration.Rule != CompositionRule.EnforceVoiceSpacing)
                .ToHashSet());

        var fugalEntryStrategy = new CompositionStrategyFactory(new NoteChoiceGenerator(), compositionRuleFactory.CreateAggregate(fugalEntryRuleConfiguration), randomProvider, logger).Create(configuration);

        var voiceRhythmLedger = new VoiceRhythmLedger();
        var voiceRhythmScheduler = voiceRhythmSchedulerOverride ?? new VoiceRhythmScheduler(configuration);
        var ornamentationEngineBuilder = new OrnamentationEngineBuilder(configuration, musicalTimeSpanCalculator, randomProvider, logger, voiceRhythmLedger);
        var compositionDecorator = new CompositionDecorator(ornamentationEngineBuilder.BuildOrnamentationEngine(), ornamentationEngineBuilder.BuildSustainedNoteEngine(), voiceRhythmScheduler, configuration);
        var dynamicsApplicator = new DynamicsApplicator(configuration, new DynamicsEngineBuilder(configuration, randomProvider).Build());
        var motifBankFactory = new MotifBankFactory(new MotifExtractor(configuration), configuration);
        var motifDeveloper = new MotifDeveloper(new MotifApplicator(configuration), weightedRandomBooleanGenerator, randomProvider, configuration);
        var cadenceClassifier = new CadenceClassifier(chordNumberIdentifier, chordInversionIdentifier, configuration);

        var cadentialTrillApplicator = new CadentialTrillApplicator(
            cadenceClassifier,
            new OrnamentationProcessorConfigurationFactory(chordNumberIdentifier, weightedRandomBooleanGenerator, configuration),
            musicalTimeSpanCalculator,
            configuration);

        var compositionPhraser = new CompositionPhraser(compositionRule, new ThemeSplitter(), weightedRandomBooleanGenerator, randomProvider, logger, configuration, motifBankFactory, motifDeveloper, cadentialTrillApplicator);
        var fugalEntryPlacer = new FugalEntryPlacer(configuration, voiceSpacingSatisfiabilityAnalyzer.GetFeasibleNoteNumbersByInstrument(configuration));
        var scoringRuleFactory = new ScoringRuleFactory(configuration, chordNumberIdentifier, chordInversionIdentifier);
        var chordSelector = new WeightedChordSelector(scoringRuleFactory.CreateAggregate(AggregateScoringRuleConfiguration.Default), randomProvider);
        var chordComposer = new ChordComposer(compositionStrategy, chordSelector, logger);
        var themeComposer = new ThemeComposer(compositionStrategy, fugalEntryStrategy, compositionDecorator, chordComposer, fugalEntryPlacer, new FugalAnswerStrategy(configuration), chordSelector, dispatcher, logger, configuration);
        var endingComposer = new EndingComposer(compositionStrategy, compositionDecorator, chordNumberIdentifier, cadenceClassifier, cadentialTrillApplicator, chordSelector, dispatcher, logger, configuration);

        var composer = new Composer(
            compositionDecorator,
            compositionPhraser,
            chordComposer,
            new HarmonicRhythmScheduler(configuration),
            voiceRhythmScheduler,
            voiceRhythmLedger,
            new SuspensionApplicator(weightedRandomBooleanGenerator, configuration),
            new TonicizationApplicator(chordNumberIdentifier, weightedRandomBooleanGenerator, configuration),
            themeComposer,
            endingComposer,
            dynamicsApplicator,
            dispatcher,
            configuration);

        return new ComposerGraph(composer, voiceRhythmLedger, configuration);
    }
}
