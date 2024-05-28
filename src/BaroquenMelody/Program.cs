using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Compositions.Phrasing;
using BaroquenMelody.Library.Compositions.Strategies;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using System.Globalization;

Console.WriteLine("Hit 'enter' to start composing...");
Console.ReadLine();

// proof of concept testing code...
var phrasingConfiguration = new PhrasingConfiguration(
    PhraseLengths: [2, 4, 8],
    MaxPhraseRepetitions: 4,
    MinPhraseRepetitionPoolSize: 10,
    PhraseRepetitionProbability: 90
);

var compositionConfiguration = new CompositionConfiguration(
    new HashSet<VoiceConfiguration>
    {
        new(Voice.Soprano, Notes.G5, Notes.C6),
        new(Voice.Alto, Notes.C4, Notes.G5),
        new(Voice.Tenor, Notes.C3, Notes.C4),
        new(Voice.Bass, Notes.C2, Notes.C3)
    },
    phrasingConfiguration,
    Scale.Parse("D dorian"),
    Meter.FourFour,
    100
);

var compositionRule = new AggregateCompositionRule(
    [
        new EnsureVoiceRange(compositionConfiguration),
        new AvoidDissonance(),
        new AvoidDissonantLeaps(compositionConfiguration),
        new AvoidRepetition()
    ]
);

var compositionStrategyFactory = new CompositionStrategyFactory(
    new ChordChoiceRepositoryFactory(
        new NoteChoiceGenerator()
    ),
    compositionRule
);

var compositionStrategy = compositionStrategyFactory.Create(compositionConfiguration);

Console.WriteLine("Done creating composition strategy!");

var compositionDecorator = new CompositionDecorator(
    new OrnamentationEngineBuilder(compositionConfiguration, new MusicalTimeSpanCalculator()).Build(),
    compositionConfiguration
);

var compositionPhraser = new CompositionPhraser(compositionRule, compositionConfiguration);

var composer = new Composer(
    compositionStrategy,
    compositionDecorator,
    compositionPhraser,
    compositionConfiguration
);

Console.WriteLine("Composing...");

var composition = composer.Compose();

Console.WriteLine("Done composing!");
Console.WriteLine("Creating MIDI file...");

// just for testing purposes, we'll create a MIDI file with 3 tracks, one for each voice
var tempoMap = TempoMap.Create(Tempo.FromBeatsPerMinute(60));

var patternBuildersByVoice = new Dictionary<Voice, PatternBuilder>
{
    { Voice.Soprano, new PatternBuilder().ProgramChange(GeneralMidiProgram.Harpsichord) },
    { Voice.Alto, new PatternBuilder().ProgramChange(GeneralMidiProgram.Harpsichord) },
    { Voice.Tenor, new PatternBuilder().ProgramChange(GeneralMidiProgram.Harpsichord) },
    { Voice.Bass, new PatternBuilder().ProgramChange(GeneralMidiProgram.Harpsichord) }
};

// Use pattern builders to add notes to the pattern for each voice...
foreach (var measure in composition.Measures)
{
    foreach (var beat in measure.Beats)
    {
        var sopranoNote = beat.Chord[Voice.Soprano];
        var altoNote = beat.Chord[Voice.Alto];
        var tenorNote = beat.Chord[Voice.Tenor];
        var bassNote = beat.Chord[Voice.Bass];

        patternBuildersByVoice[Voice.Soprano].SetNoteLength(sopranoNote.Duration).Note(sopranoNote.Raw);

        foreach (var ornamentation in sopranoNote.Ornamentations)
        {
            patternBuildersByVoice[Voice.Soprano].SetNoteLength(ornamentation.Duration).Note(ornamentation.Raw);
        }

        patternBuildersByVoice[Voice.Alto].SetNoteLength(altoNote.Duration).Note(altoNote.Raw);

        foreach (var ornamentation in altoNote.Ornamentations)
        {
            patternBuildersByVoice[Voice.Alto].SetNoteLength(ornamentation.Duration).Note(ornamentation.Raw);
        }

        patternBuildersByVoice[Voice.Tenor].SetNoteLength(tenorNote.Duration).Note(tenorNote.Raw);

        foreach (var ornamentation in tenorNote.Ornamentations)
        {
            patternBuildersByVoice[Voice.Tenor].SetNoteLength(ornamentation.Duration).Note(ornamentation.Raw);
        }

        patternBuildersByVoice[Voice.Bass].SetNoteLength(bassNote.Duration).Note(bassNote.Raw);

        foreach (var ornamentation in bassNote.Ornamentations)
        {
            patternBuildersByVoice[Voice.Bass].SetNoteLength(ornamentation.Duration).Note(ornamentation.Raw);
        }
    }
}

var midiFile = new MidiFile(
    patternBuildersByVoice[Voice.Soprano].Build().ToTrackChunk(tempoMap, (FourBitNumber)1),
    patternBuildersByVoice[Voice.Alto].Build().ToTrackChunk(tempoMap, (FourBitNumber)1),
    patternBuildersByVoice[Voice.Tenor].Build().ToTrackChunk(tempoMap, (FourBitNumber)1),
    patternBuildersByVoice[Voice.Bass].Build().ToTrackChunk(tempoMap, (FourBitNumber)1)
);

// save the MIDI file with a timestamp in the filename to avoid overwriting on subsequent test runs...
var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

midiFile.Write($"test-{timestamp}.mid");

Console.WriteLine("Done creating MIDI file!");

Console.WriteLine("Press any key to exit...");
Console.ReadLine();
