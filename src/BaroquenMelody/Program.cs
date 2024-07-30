using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Compositions.Phrasing;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Random;
using BaroquenMelody.Library.Midi;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using Microsoft.Extensions.Logging;
using System.Globalization;

Console.WriteLine("Hit 'enter' to start composing...");
Console.ReadLine();
Console.WriteLine();

var phrasingConfiguration = new PhrasingConfiguration(
    PhraseLengths: [1, 2, 3, 4, 5, 6, 7, 8],
    MaxPhraseRepetitions: 8,
    MinPhraseRepetitionPoolSize: 2,
    PhraseRepetitionProbability: 100
);

var compositionConfiguration = new CompositionConfiguration(
    new HashSet<VoiceConfiguration>
    {
        new(Voice.Soprano, Notes.C6, Notes.E7, GeneralMidi2Program.ChurchOrganOctaveMix),
        new(Voice.Alto, Notes.G4, Notes.B5, GeneralMidi2Program.ChurchOrganOctaveMix),
        new(Voice.Tenor, Notes.F3, Notes.A4, GeneralMidi2Program.ChurchOrganOctaveMix),
        new(Voice.Bass, Notes.C2, Notes.E3, GeneralMidi2Program.ChurchOrganOctaveMix)
    },
    phrasingConfiguration,
    AggregateCompositionRuleConfiguration.Default,
    BaroquenScale.Parse("C Major"),
    Meter.FourFour,
    25
);

using var factory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = factory.CreateLogger<Composer>();

var compositionRuleFactory = new CompositionRuleFactory(compositionConfiguration, new WeightedRandomBooleanGenerator());
var compositionRule = compositionRuleFactory.CreateAggregate(compositionConfiguration.AggregateCompositionRuleConfiguration);

var compositionStrategyFactory = new CompositionStrategyFactory(
    new ChordChoiceRepositoryFactory(
        new NoteChoiceGenerator()
    ),
    compositionRule,
    logger
);

var compositionStrategy = compositionStrategyFactory.Create(compositionConfiguration);
var ornamentationEngineBuilder = new OrnamentationEngineBuilder(compositionConfiguration, new MusicalTimeSpanCalculator(), logger);

var compositionDecorator = new CompositionDecorator(
    ornamentationEngineBuilder.BuildOrnamentationEngine(),
    ornamentationEngineBuilder.BuildSustainedNoteEngine(),
    compositionConfiguration
);

var weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator();
var themeSplitter = new ThemeSplitter();
var compositionPhraser = new CompositionPhraser(compositionRule, themeSplitter, weightedRandomBooleanGenerator, logger, compositionConfiguration);
var noteTransposer = new NoteTransposer(compositionConfiguration);
var chordComposer = new ChordComposer(compositionStrategy, compositionConfiguration, logger);
var chordNumberIdentifier = new ChordNumberIdentifier(compositionConfiguration);

var themeComposer = new ThemeComposer(
    compositionStrategy,
    compositionDecorator,
    chordComposer,
    noteTransposer,
    logger,
    compositionConfiguration
);

var endingComposer = new EndingComposer(
    compositionStrategy,
    compositionDecorator,
    chordNumberIdentifier,
    logger,
    compositionConfiguration
);

var composer = new Composer(
    compositionDecorator,
    compositionPhraser,
    chordComposer,
    themeComposer,
    endingComposer,
    logger,
    compositionConfiguration
);

var stopwatch = System.Diagnostics.Stopwatch.StartNew();

var composition = composer.Compose();

stopwatch.Stop();

Console.WriteLine();
Console.WriteLine($"Done composing! Elapsed time: {stopwatch.Elapsed}.");
Console.WriteLine("Creating MIDI file...");

var midiGenerator = new MidiGenerator(compositionConfiguration);
var midiFile = midiGenerator.Generate(composition);
var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

midiFile.Write($"test-{timestamp}.mid");

Console.WriteLine("Done creating MIDI file!");
Console.WriteLine("Press any key to exit...");
Console.ReadLine();
