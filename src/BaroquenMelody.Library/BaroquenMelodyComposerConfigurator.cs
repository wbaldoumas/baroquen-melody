using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Dynamics.Engine.Builders;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms;
using BaroquenMelody.Library.Logging;
using BaroquenMelody.Library.Midi;
using BaroquenMelody.Library.Motifs;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
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
using System.Collections.Frozen;

namespace BaroquenMelody.Library;

/// <summary>
///     Centralized logic for configuring a <see cref="MidiFileComposer"/> which can generate a <see cref="MidiFileComposition"/>.
/// </summary>
/// <param name="logger">A logger to be used throughout the composition process.</param>
/// <param name="dispatcher">A dispatcher to be used to dispatch actions to the store.</param>
/// <param name="randomProvider">The random provider threaded through the composition graph so randomness is reproducible under a seed.</param>
/// <param name="voiceSpacingSatisfiabilityAnalyzer">Determines whether the voice spacing rule can be satisfied by the configured instrument ranges, so an unsatisfiable rule can be skipped instead of dead-ending the composition.</param>
internal sealed class BaroquenMelodyComposerConfigurator(
    ILogger<MidiFileComposition> logger,
    IDispatcher dispatcher,
    IRandomProvider randomProvider,
    IVoiceSpacingSatisfiabilityAnalyzer voiceSpacingSatisfiabilityAnalyzer
) : IBaroquenMelodyComposerConfigurator
{
    private readonly IMusicalTimeSpanCalculator _musicalTimeSpanCalculator = new MusicalTimeSpanCalculator();

    private readonly INoteChoiceGenerator _noteChoiceGenerator = new NoteChoiceGenerator();

    private readonly IWeightedRandomBooleanGenerator _weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator(randomProvider);

    private readonly IThemeSplitter _themeSplitter = new ThemeSplitter();

    public IMidiFileComposer Configure(CompositionConfiguration compositionConfiguration)
    {
        var chordNumberIdentifier = new ChordNumberIdentifier(compositionConfiguration);
        var chordInversionIdentifier = new ChordInversionIdentifier(chordNumberIdentifier, compositionConfiguration);
        var compositionRuleFactory = new CompositionRuleFactory(compositionConfiguration, _weightedRandomBooleanGenerator, chordNumberIdentifier);
        var effectiveRuleConfiguration = ResolveEffectiveRuleConfiguration(compositionConfiguration);
        var compositionRule = compositionRuleFactory.CreateAggregate(effectiveRuleConfiguration);
        var compositionStrategyFactory = new CompositionStrategyFactory(_noteChoiceGenerator, compositionRule, randomProvider, logger);
        var compositionStrategy = compositionStrategyFactory.Create(compositionConfiguration);

        // The fugal entries pin one voice's notes chord by chord; with four voices the spacing rule leaves those
        // pinned chords near-zero rule-satisfying candidates, which silently degrades every composition to the
        // fallback theme. The exposition therefore takes the textbook liberty: its pinned entry chords are checked
        // without the spacing rule (entry placement still steers toward spacing-feasible registers), while the
        // initial measure, the bridge back to the body, and everything after enforce it in full.
        var fugalEntryRuleConfiguration = new AggregateCompositionRuleConfiguration(
            effectiveRuleConfiguration.Configurations
                .Where(static configuration => configuration.Rule != CompositionRule.EnforceVoiceSpacing)
                .ToHashSet()
        );

        var fugalEntryCompositionRule = compositionRuleFactory.CreateAggregate(fugalEntryRuleConfiguration);
        var fugalEntryCompositionStrategy = new CompositionStrategyFactory(_noteChoiceGenerator, fugalEntryCompositionRule, randomProvider, logger).Create(compositionConfiguration);
        var ornamentationEngineBuilder = new OrnamentationEngineBuilder(compositionConfiguration, _musicalTimeSpanCalculator, randomProvider, logger);
        var dynamicsEngineBuilder = new DynamicsEngineBuilder(compositionConfiguration, randomProvider);
        var dynamicsApplicator = new DynamicsApplicator(compositionConfiguration, dynamicsEngineBuilder.Build());
        var compositionDecorator = new CompositionDecorator(ornamentationEngineBuilder.BuildOrnamentationEngine(), ornamentationEngineBuilder.BuildSustainedNoteEngine(), compositionConfiguration);
        var motifExtractor = new MotifExtractor(compositionConfiguration);
        var motifApplicator = new MotifApplicator(compositionConfiguration);
        var motifBankFactory = new MotifBankFactory(motifExtractor, compositionConfiguration);
        var motifDeveloper = new MotifDeveloper(motifApplicator, _weightedRandomBooleanGenerator, randomProvider, compositionConfiguration);
        var cadenceClassifier = new CadenceClassifier(chordNumberIdentifier, chordInversionIdentifier, compositionConfiguration);
        var cadentialTrillApplicator = new CadentialTrillApplicator(
            cadenceClassifier,
            new OrnamentationProcessorConfigurationFactory(chordNumberIdentifier, _weightedRandomBooleanGenerator, compositionConfiguration, logger),
            _musicalTimeSpanCalculator,
            compositionConfiguration);
        var compositionPhraser = new CompositionPhraser(compositionRule, _themeSplitter, _weightedRandomBooleanGenerator, randomProvider, logger, compositionConfiguration, motifBankFactory, motifDeveloper, cadentialTrillApplicator);
        var fugalEntryPlacer = new FugalEntryPlacer(compositionConfiguration, ResolveSpacingFeasibleNoteNumbers(compositionConfiguration, effectiveRuleConfiguration));
        var fugalAnswerStrategy = new FugalAnswerStrategy(compositionConfiguration);
        var scoringRuleFactory = new ScoringRuleFactory(compositionConfiguration, chordNumberIdentifier, chordInversionIdentifier);
        var aggregateScoringRule = scoringRuleFactory.CreateAggregate(compositionConfiguration.AggregateScoringRuleConfiguration ?? AggregateScoringRuleConfiguration.Default);
        var chordSelector = new WeightedChordSelector(aggregateScoringRule, randomProvider);
        var chordComposer = new ChordComposer(compositionStrategy, chordSelector, logger);
        var themeComposer = new ThemeComposer(compositionStrategy, fugalEntryCompositionStrategy, compositionDecorator, chordComposer, fugalEntryPlacer, fugalAnswerStrategy, chordSelector, dispatcher, logger, compositionConfiguration);
        var endingComposer = new EndingComposer(compositionStrategy, compositionDecorator, chordNumberIdentifier, cadenceClassifier, cadentialTrillApplicator, chordSelector, dispatcher, logger, compositionConfiguration);
        var harmonicRhythmScheduler = new HarmonicRhythmScheduler(compositionConfiguration);
        var suspensionApplicator = new SuspensionApplicator(_weightedRandomBooleanGenerator, compositionConfiguration);
        var tonicizationApplicator = new TonicizationApplicator(chordNumberIdentifier, _weightedRandomBooleanGenerator, compositionConfiguration);
        var composer = new Composer(compositionDecorator, compositionPhraser, chordComposer, harmonicRhythmScheduler, suspensionApplicator, tonicizationApplicator, themeComposer, endingComposer, dynamicsApplicator, dispatcher, compositionConfiguration);
        var midiGenerator = new MidiGenerator(compositionConfiguration);

        IComposer effectiveComposer = composer;

        // The ground bass form replaces the whole fugal pipeline with its own composer, keeping the standard
        // composer as its fallback for bass ranges no ground can anchor in. When the form is disabled the
        // object graph is exactly the pre-form graph. The home section's components alias the singletons
        // above, so a home-only plan composes through exactly the pre-modulation objects.
        if ((compositionConfiguration.GroundBassConfiguration ?? GroundBassConfiguration.Default).Enabled)
        {
            var groundBassPlanner = new GroundBassPlanner(compositionConfiguration, randomProvider);
            var homeComponents = new GroundBassSectionComponents(compositionStrategy, compositionStrategy, chordSelector, compositionDecorator, tonicizationApplicator);
            var relativeComponents = default(GroundBassSectionComponents);

            // The planner journeys only in the lifted modes with the toggle on; this gate is deliberately
            // WIDER than the planner's (which also weighs statement counts, range feasibility, and seam
            // legality), so relative components exist whenever a foreign section can possibly be planned.
            if ((compositionConfiguration.GroundBassConfiguration ?? GroundBassConfiguration.Default).Modulate
                && compositionConfiguration.Mode is Mode.Ionian or Mode.Aeolian)
            {
                var seamRuleConfiguration = new AggregateCompositionRuleConfiguration(
                    effectiveRuleConfiguration.Configurations
                        .Where(static configuration => configuration.Rule != CompositionRule.FollowStandardChordProgression)
                        .ToHashSet()
                );

                var homeSeamRule = compositionRuleFactory.CreateAggregate(seamRuleConfiguration);
                var homeSeamStrategy = new CompositionStrategyFactory(_noteChoiceGenerator, homeSeamRule, randomProvider, logger).Create(compositionConfiguration);

                homeComponents = homeComponents with { SeamStrategy = homeSeamStrategy };

                // Relative scales share the home scale's pitch set, so the spacing-satisfiability verdict
                // and the effective rule set carry over unchanged; only Tonic and Mode move. The relative
                // configuration must be constructed fresh - a `with` clone would keep the home Scale, which
                // property initializers compute once at construction.
                var relativeConfiguration = BuildRelativeConfiguration(compositionConfiguration);
                var relativeChordNumberIdentifier = new ChordNumberIdentifier(relativeConfiguration);
                var relativeChordInversionIdentifier = new ChordInversionIdentifier(relativeChordNumberIdentifier, relativeConfiguration);
                var relativeRuleFactory = new CompositionRuleFactory(relativeConfiguration, _weightedRandomBooleanGenerator, relativeChordNumberIdentifier);
                var relativeRule = relativeRuleFactory.CreateAggregate(effectiveRuleConfiguration);
                var relativeSeamRule = relativeRuleFactory.CreateAggregate(seamRuleConfiguration);
                var relativeStrategy = new CompositionStrategyFactory(_noteChoiceGenerator, relativeRule, randomProvider, logger).Create(relativeConfiguration);
                var relativeSeamStrategy = new CompositionStrategyFactory(_noteChoiceGenerator, relativeSeamRule, randomProvider, logger).Create(relativeConfiguration);
                var relativeScoringRuleFactory = new ScoringRuleFactory(relativeConfiguration, relativeChordNumberIdentifier, relativeChordInversionIdentifier);
                var relativeScoringRule = relativeScoringRuleFactory.CreateAggregate(compositionConfiguration.AggregateScoringRuleConfiguration ?? AggregateScoringRuleConfiguration.Default);
                var relativeChordSelector = new WeightedChordSelector(relativeScoringRule, randomProvider);
                var relativeOrnamentationEngineBuilder = new OrnamentationEngineBuilder(relativeConfiguration, _musicalTimeSpanCalculator, randomProvider, logger);
                var relativeDecorator = new CompositionDecorator(relativeOrnamentationEngineBuilder.BuildOrnamentationEngine(), relativeOrnamentationEngineBuilder.BuildSustainedNoteEngine(), relativeConfiguration);
                var relativeTonicizationApplicator = new TonicizationApplicator(relativeChordNumberIdentifier, _weightedRandomBooleanGenerator, relativeConfiguration);

                relativeComponents = new GroundBassSectionComponents(relativeStrategy, relativeSeamStrategy, relativeChordSelector, relativeDecorator, relativeTonicizationApplicator);
            }

            effectiveComposer = new GroundBassComposer(
                groundBassPlanner,
                homeComponents,
                relativeComponents,
                compositionRule,
                suspensionApplicator,
                cadenceClassifier,
                chordNumberIdentifier,
                cadentialTrillApplicator,
                dynamicsApplicator,
                composer,
                dispatcher,
                logger,
                compositionConfiguration);
        }

        return new MidiFileComposer(effectiveComposer, midiGenerator);
    }

    /// <summary>
    ///     Builds the relative key's configuration: the same composition in every respect except its tonal
    ///     center - the submediant's Aeolian for an Ionian home, the mediant's Ionian for an Aeolian home.
    ///     Constructed through the full constructor so the property-initialized <see cref="CompositionConfiguration.Scale"/>
    ///     binds to the relative key (a `with` clone would carry the home scale).
    /// </summary>
    private static CompositionConfiguration BuildRelativeConfiguration(CompositionConfiguration compositionConfiguration)
    {
        var (relativeTonic, relativeMode) = compositionConfiguration.Mode == Mode.Ionian
            ? (compositionConfiguration.Scale.Submediant, Mode.Aeolian)
            : (compositionConfiguration.Scale.Mediant, Mode.Ionian);

        return new CompositionConfiguration(
            compositionConfiguration.InstrumentConfigurations,
            compositionConfiguration.PhrasingConfiguration,
            compositionConfiguration.AggregateCompositionRuleConfiguration,
            compositionConfiguration.AggregateOrnamentationConfiguration,
            relativeTonic,
            relativeMode,
            compositionConfiguration.Meter,
            compositionConfiguration.DefaultNoteTimeSpan,
            compositionConfiguration.MinimumMeasures,
            compositionConfiguration.CompositionContextSize,
            compositionConfiguration.Tempo,
            compositionConfiguration.ShuffleOrnamentationProcessors,
            compositionConfiguration.MaxLookAheadDepth,
            compositionConfiguration.AggregateScoringRuleConfiguration,
            compositionConfiguration.MotifDevelopmentConfiguration,
            compositionConfiguration.HarmonicRhythmConfiguration,
            compositionConfiguration.SuspensionConfiguration,
            compositionConfiguration.TonicizationConfiguration,
            compositionConfiguration.GroundBassConfiguration);
    }

    /// <summary>
    ///     Resolves the spacing-feasible note numbers used to steer fugal entry placement: the exposition's pinned
    ///     entry chords are validated without the spacing rule, so placing entries inside the feasible registers is
    ///     what keeps the theme aligned with the spacing the rest of the composition enforces. Null when the rule
    ///     does not constrain placement.
    /// </summary>
    private FrozenDictionary<Instrument, FrozenSet<int>>? ResolveSpacingFeasibleNoteNumbers(
        CompositionConfiguration compositionConfiguration,
        AggregateCompositionRuleConfiguration effectiveRuleConfiguration
    ) => effectiveRuleConfiguration.Configurations.Any(static configuration => configuration is { Rule: CompositionRule.EnforceVoiceSpacing, IsEnabled: true })
        ? voiceSpacingSatisfiabilityAnalyzer.GetFeasibleNoteNumbersByInstrument(compositionConfiguration)
        : null;

    /// <summary>
    ///     Disables the voice spacing rule when the configured instrument ranges make it impossible to satisfy, since
    ///     enforcing an unsatisfiable rule would leave the composer without a single valid chord and dead-end the
    ///     composition. The passed-in configuration is left untouched; only the composer's effective rule set changes.
    /// </summary>
    private AggregateCompositionRuleConfiguration ResolveEffectiveRuleConfiguration(CompositionConfiguration compositionConfiguration)
    {
        var aggregateRuleConfiguration = compositionConfiguration.AggregateCompositionRuleConfiguration;

        var voiceSpacingConfiguration = aggregateRuleConfiguration.Configurations
            .FirstOrDefault(static configuration => configuration.Rule == CompositionRule.EnforceVoiceSpacing);

        if (voiceSpacingConfiguration is not { IsEnabled: true } || voiceSpacingSatisfiabilityAnalyzer.IsSatisfiable(compositionConfiguration))
        {
            return aggregateRuleConfiguration;
        }

        logger.LogVoiceSpacingUnsatisfiable();

        return new AggregateCompositionRuleConfiguration(
            aggregateRuleConfiguration.Configurations
                .Where(static configuration => configuration.Rule != CompositionRule.EnforceVoiceSpacing)
                .Append(voiceSpacingConfiguration with { Status = ConfigurationStatus.Disabled })
                .ToHashSet()
        );
    }
}
