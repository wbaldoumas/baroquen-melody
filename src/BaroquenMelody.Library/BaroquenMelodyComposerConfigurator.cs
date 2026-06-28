using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Dynamics.Engine.Builders;
using BaroquenMelody.Library.Midi;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Phrasing;
using BaroquenMelody.Library.Rules;
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
internal sealed class BaroquenMelodyComposerConfigurator(ILogger<MidiFileComposition> logger, IDispatcher dispatcher, IRandomProvider randomProvider) : IBaroquenMelodyComposerConfigurator
{
    private readonly IMusicalTimeSpanCalculator _musicalTimeSpanCalculator = new MusicalTimeSpanCalculator();

    private readonly IChordChoiceRepositoryFactory _chordChoiceRepositoryFactory = new ChordChoiceRepositoryFactory(new NoteChoiceGenerator());

    private readonly IWeightedRandomBooleanGenerator _weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator(randomProvider);

    private readonly IThemeSplitter _themeSplitter = new ThemeSplitter();

    public IMidiFileComposer Configure(CompositionConfiguration compositionConfiguration)
    {
        var chordNumberIdentifier = new ChordNumberIdentifier(compositionConfiguration);
        var compositionRuleFactory = new CompositionRuleFactory(compositionConfiguration, _weightedRandomBooleanGenerator, chordNumberIdentifier);
        var compositionRule = compositionRuleFactory.CreateAggregate(compositionConfiguration.AggregateCompositionRuleConfiguration);
        var compositionStrategyFactory = new CompositionStrategyFactory(_chordChoiceRepositoryFactory, compositionRule, randomProvider, logger);
        var compositionStrategy = compositionStrategyFactory.Create(compositionConfiguration);
        var ornamentationEngineBuilder = new OrnamentationEngineBuilder(compositionConfiguration, _musicalTimeSpanCalculator, randomProvider, logger);
        var dynamicsEngineBuilder = new DynamicsEngineBuilder(compositionConfiguration, randomProvider);
        var dynamicsApplicator = new DynamicsApplicator(compositionConfiguration, dynamicsEngineBuilder.Build());
        var compositionDecorator = new CompositionDecorator(ornamentationEngineBuilder.BuildOrnamentationEngine(), ornamentationEngineBuilder.BuildSustainedNoteEngine(), compositionConfiguration);
        var compositionPhraser = new CompositionPhraser(compositionRule, _themeSplitter, _weightedRandomBooleanGenerator, randomProvider, logger, compositionConfiguration);
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
}
