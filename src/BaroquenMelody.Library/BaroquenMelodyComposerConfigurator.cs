using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Dynamics.Engine.Builders;
using BaroquenMelody.Library.Logging;
using BaroquenMelody.Library.Midi;
using BaroquenMelody.Library.Motifs;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Phrasing;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Scoring;
using BaroquenMelody.Library.Strategies;
using Fluxor;
using Microsoft.Extensions.Logging;

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
        var compositionRuleFactory = new CompositionRuleFactory(compositionConfiguration, _weightedRandomBooleanGenerator, chordNumberIdentifier);
        var compositionRule = compositionRuleFactory.CreateAggregate(ResolveEffectiveRuleConfiguration(compositionConfiguration));
        var compositionStrategyFactory = new CompositionStrategyFactory(_noteChoiceGenerator, compositionRule, randomProvider, logger);
        var compositionStrategy = compositionStrategyFactory.Create(compositionConfiguration);
        var ornamentationEngineBuilder = new OrnamentationEngineBuilder(compositionConfiguration, _musicalTimeSpanCalculator, randomProvider, logger);
        var dynamicsEngineBuilder = new DynamicsEngineBuilder(compositionConfiguration, randomProvider);
        var dynamicsApplicator = new DynamicsApplicator(compositionConfiguration, dynamicsEngineBuilder.Build());
        var compositionDecorator = new CompositionDecorator(ornamentationEngineBuilder.BuildOrnamentationEngine(), ornamentationEngineBuilder.BuildSustainedNoteEngine(), compositionConfiguration);
        var motifExtractor = new MotifExtractor(compositionConfiguration);
        var motifApplicator = new MotifApplicator(compositionConfiguration);
        var motifBankFactory = new MotifBankFactory(motifExtractor, compositionConfiguration);
        var motifDeveloper = new MotifDeveloper(motifApplicator, _weightedRandomBooleanGenerator, randomProvider, compositionConfiguration);
        var compositionPhraser = new CompositionPhraser(compositionRule, _themeSplitter, _weightedRandomBooleanGenerator, randomProvider, logger, compositionConfiguration, motifBankFactory, motifDeveloper);
        var fugalEntryPlacer = new FugalEntryPlacer(compositionConfiguration);
        var fugalAnswerStrategy = new FugalAnswerStrategy(compositionConfiguration);
        var scoringRuleFactory = new ScoringRuleFactory(compositionConfiguration);
        var aggregateScoringRule = scoringRuleFactory.CreateAggregate(compositionConfiguration.AggregateScoringRuleConfiguration ?? AggregateScoringRuleConfiguration.Default);
        var chordSelector = new WeightedChordSelector(aggregateScoringRule, randomProvider);
        var chordComposer = new ChordComposer(compositionStrategy, chordSelector, logger);
        var themeComposer = new ThemeComposer(compositionStrategy, compositionDecorator, chordComposer, fugalEntryPlacer, fugalAnswerStrategy, chordSelector, dispatcher, logger, compositionConfiguration);
        var endingComposer = new EndingComposer(compositionStrategy, compositionDecorator, chordNumberIdentifier, chordSelector, dispatcher, logger, compositionConfiguration);
        var composer = new Composer(compositionDecorator, compositionPhraser, chordComposer, themeComposer, endingComposer, dynamicsApplicator, dispatcher, compositionConfiguration);
        var midiGenerator = new MidiGenerator(compositionConfiguration);

        return new MidiFileComposer(composer, midiGenerator);
    }

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
                .Append(new CompositionRuleConfiguration(CompositionRule.EnforceVoiceSpacing, ConfigurationStatus.Disabled, voiceSpacingConfiguration.Strictness))
                .ToHashSet()
        );
    }
}
