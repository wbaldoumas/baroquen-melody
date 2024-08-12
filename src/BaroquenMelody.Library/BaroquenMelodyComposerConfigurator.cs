using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Midi;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Compositions.Phrasing;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Random;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library;

/// <summary>
///     Centralized logic for configuring a <see cref="BaroquenMelodyComposer"/> which can generate a <see cref="BaroquenMelody"/>.
/// </summary>
/// <param name="logger">A logger to be used throughout the composition process.</param>
/// <param name="dispatcher">A dispatcher to be used to dispatch actions to the store.</param>
public sealed class BaroquenMelodyComposerConfigurator(ILogger<BaroquenMelody> logger, IDispatcher dispatcher) : IBaroquenMelodyComposerConfigurator
{
    private readonly IMusicalTimeSpanCalculator _musicalTimeSpanCalculator = new MusicalTimeSpanCalculator();

    private readonly IChordChoiceRepositoryFactory _chordChoiceRepositoryFactory = new ChordChoiceRepositoryFactory(new NoteChoiceGenerator());

    private readonly IWeightedRandomBooleanGenerator _weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator();

    private readonly IThemeSplitter _themeSplitter = new ThemeSplitter();

    public IBaroquenMelodyComposer Configure(CompositionConfiguration compositionConfiguration)
    {
        var chordNumberIdentifier = new ChordNumberIdentifier(compositionConfiguration);
        var compositionRuleFactory = new CompositionRuleFactory(compositionConfiguration, _weightedRandomBooleanGenerator, chordNumberIdentifier);
        var compositionRule = compositionRuleFactory.CreateAggregate(compositionConfiguration.AggregateCompositionRuleConfiguration);
        var compositionStrategyFactory = new CompositionStrategyFactory(_chordChoiceRepositoryFactory, compositionRule, logger);
        var compositionStrategy = compositionStrategyFactory.Create(compositionConfiguration);
        var ornamentationEngineBuilder = new OrnamentationEngineBuilder(compositionConfiguration, _musicalTimeSpanCalculator, logger);
        var compositionDecorator = new CompositionDecorator(ornamentationEngineBuilder.BuildOrnamentationEngine(), ornamentationEngineBuilder.BuildSustainedNoteEngine(), compositionConfiguration);
        var compositionPhraser = new CompositionPhraser(compositionRule, _themeSplitter, _weightedRandomBooleanGenerator, logger, compositionConfiguration);
        var noteTransposer = new NoteTransposer(compositionConfiguration);
        var chordComposer = new ChordComposer(compositionStrategy, compositionConfiguration, logger);
        var themeComposer = new ThemeComposer(compositionStrategy, compositionDecorator, chordComposer, noteTransposer, logger, compositionConfiguration);
        var endingComposer = new EndingComposer(compositionStrategy, compositionDecorator, chordNumberIdentifier, logger, compositionConfiguration);
        var composer = new Composer(compositionDecorator, compositionPhraser, chordComposer, themeComposer, endingComposer, dispatcher, compositionConfiguration);
        var midiGenerator = new MidiGenerator(compositionConfiguration);

        return new BaroquenMelodyComposer(composer, midiGenerator);
    }
}
