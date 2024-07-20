using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Compositions.Phrasing;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Compositions.Strategies;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using System.Globalization;
using Interval = BaroquenMelody.Library.Compositions.MusicTheory.Enums.Interval;

// proof of concept testing code...
Console.WriteLine("Hit 'enter' to start composing...");
Console.ReadLine();

var phrasingConfiguration = new PhrasingConfiguration(
    PhraseLengths: [1, 2, 3, 4, 5, 6, 7, 8],
    MaxPhraseRepetitions: 8,
    MinPhraseRepetitionPoolSize: 2,
    PhraseRepetitionProbability: 100
);

var compositionConfiguration = new CompositionConfiguration(
    new HashSet<VoiceConfiguration>
    {
        new(Voice.Soprano, Notes.C4, Notes.G5),
        new(Voice.Alto, Notes.C3, Notes.G4),
        new(Voice.Tenor, Notes.C2, Notes.G3),
        new(Voice.Bass, Notes.C1, Notes.G2)
    },
    phrasingConfiguration,
    BaroquenScale.Parse("D Dorian"),
    Meter.FourFour,
    25
);

var compositionRule = new AggregateCompositionRule(
    [
        new HandleAscendingSeventh(compositionConfiguration),
        new EnsureVoiceRange(compositionConfiguration),
        new AvoidDissonance(),
        new AvoidDissonantLeaps(compositionConfiguration),
        new AvoidRepetition(),
        new AvoidParallelIntervals(Interval.PerfectFifth),
        new AvoidParallelIntervals(Interval.PerfectFourth),
        new AvoidParallelIntervals(Interval.Unison),
        new AvoidOverDoubling(),
        new FollowsStandardProgression(compositionConfiguration),
        new AvoidDirectIntervals(Interval.PerfectFifth),
        new AvoidDirectIntervals(Interval.PerfectFourth),
        new AvoidDirectIntervals(Interval.Unison)
    ]
);

var compositionStrategyFactory = new CompositionStrategyFactory(
    new ChordChoiceRepositoryFactory(
        new NoteChoiceGenerator()
    ),
    compositionRule,
    new ChordNumberIdentifier(compositionConfiguration)
);

var compositionStrategy = compositionStrategyFactory.Create(compositionConfiguration);

var ornamentationEngineBuilder = new OrnamentationEngineBuilder(compositionConfiguration, new MusicalTimeSpanCalculator());

var compositionDecorator = new CompositionDecorator(
    ornamentationEngineBuilder.BuildOrnamentationEngine(),
    ornamentationEngineBuilder.BuildSustainedNoteEngine(),
    compositionConfiguration
);

var compositionPhraser = new CompositionPhraser(compositionRule, compositionConfiguration);
var noteTransposer = new NoteTransposer(compositionConfiguration);
var chordComposer = new ChordComposer(compositionStrategy, compositionConfiguration);

var themeComposer = new ThemeComposer(
    compositionStrategy,
    compositionDecorator,
    chordComposer,
    noteTransposer,
    compositionConfiguration
);

var endingComposer = new EndingComposer(
    compositionStrategy,
    compositionDecorator,
    compositionConfiguration
);

var composer = new Composer(
    compositionDecorator,
    compositionPhraser,
    chordComposer,
    themeComposer,
    endingComposer,
    compositionConfiguration
);

Console.WriteLine("Composing...");

var stopwatch = System.Diagnostics.Stopwatch.StartNew();

var composition = composer.Compose();

stopwatch.Stop();

Console.WriteLine($"Done composing! Elapsed time: {stopwatch.Elapsed}.");
Console.WriteLine("Creating MIDI file...");

// just for testing purposes, we'll create a MIDI file with 4 tracks, one for each voice
var tempoMap = TempoMap.Create(Tempo.FromBeatsPerMinute(60));

var patternBuildersByVoice = new Dictionary<Voice, PatternBuilder>
{
    { Voice.Soprano, new PatternBuilder().ProgramChange(GeneralMidiProgram.ChurchOrgan) },
    { Voice.Alto, new PatternBuilder().ProgramChange(GeneralMidiProgram.ChurchOrgan) },
    { Voice.Tenor, new PatternBuilder().ProgramChange(GeneralMidiProgram.ChurchOrgan) },
    { Voice.Bass, new PatternBuilder().ProgramChange(GeneralMidiProgram.ChurchOrgan) }
};

// Use pattern builders to add notes to the pattern for each voice...
foreach (var measure in composition.Measures)
{
    foreach (var beat in measure.Beats)
    {
        foreach (var voice in compositionConfiguration.Voices)
        {
            if (beat.Chord.Notes.TrueForAll(note => note.Voice != voice))
            {
                patternBuildersByVoice[voice].StepForward(MusicalTimeSpan.Quarter);

                continue;
            }

            var note = beat[voice];

            if (note.OrnamentationType != OrnamentationType.MidSustain)
            {
                if (note.OrnamentationType == OrnamentationType.Rest)
                {
                    patternBuildersByVoice[voice].StepForward(MusicalTimeSpan.Quarter);

                    continue;
                }

                patternBuildersByVoice[voice].SetNoteLength(note.Duration).Note(note.Raw);

                foreach (var ornamentation in note.Ornamentations)
                {
                    patternBuildersByVoice[voice].SetNoteLength(ornamentation.Duration).Note(ornamentation.Raw);
                }
            }
        }
    }
}

var midiFile = new MidiFile(
    patternBuildersByVoice[Voice.Soprano].Build().ToTrackChunk(tempoMap, (FourBitNumber)1),
    patternBuildersByVoice[Voice.Alto].Build().ToTrackChunk(tempoMap, (FourBitNumber)1),
    patternBuildersByVoice[Voice.Tenor].Build().ToTrackChunk(tempoMap, (FourBitNumber)1),
    patternBuildersByVoice[Voice.Bass].Build().ToTrackChunk(tempoMap, (FourBitNumber)1)
);

midiFile.ReplaceTempoMap(tempoMap);

// save the MIDI file with a timestamp in the filename to avoid overwriting on subsequent test runs...
var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

midiFile.Write($"test-{timestamp}.mid");

Console.WriteLine("Done creating MIDI file!");

Console.WriteLine("Press any key to exit...");
Console.ReadLine();
