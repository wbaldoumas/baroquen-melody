using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using BaroquenMelody.Library.Compositions.Strategies;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using System.Globalization;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

// proof of concept testing code...
var compositionConfiguration = new CompositionConfiguration(
    new HashSet<VoiceConfiguration>
    {
        new(Voice.Soprano, Note.Get(NoteName.G, 4), Note.Get(NoteName.C, 6)),
        new(Voice.Alto, Note.Get(NoteName.C, 3), Note.Get(NoteName.G, 4)),
        new(Voice.Tenor, Note.Get(NoteName.G, 2), Note.Get(NoteName.C, 3)),
        new(Voice.Bass, Note.Get(NoteName.C, 1), Note.Get(NoteName.G, 2))
    },
    Scale.Parse("C Harmonic Minor"),
    Meter.FourFour,
    100
);

var compositionStrategyFactory = new CompositionStrategyFactory(
    new ChordChoiceRepositoryFactory(
        new NoteChoiceGenerator()
    ),
    new AggregateCompositionRule([new EnsureVoiceRange(compositionConfiguration), new AvoidDissonance()])
);

var compositionStrategy = compositionStrategyFactory.Create(compositionConfiguration);

Console.WriteLine("Done creating composition strategy!");

var composer = new Composer(
    compositionStrategy,
    compositionConfiguration
);

Console.WriteLine("Composing...");

var composition = composer.Compose();

Console.WriteLine("Done composing!");
Console.WriteLine("Creating MIDI file...");

// just for testing purposes, we'll create a MIDI file with 3 tracks, one for each voice
var tempoMap = TempoMap.Default;

var patternBuildersByVoice = new Dictionary<Voice, PatternBuilder>
{
    { Voice.Soprano, new PatternBuilder().ProgramChange(GeneralMidiProgram.Harpsichord).SetNoteLength(MusicalTimeSpan.Quarter) },
    { Voice.Alto, new PatternBuilder().ProgramChange(GeneralMidiProgram.Harpsichord).SetNoteLength(MusicalTimeSpan.Quarter) },
    { Voice.Tenor, new PatternBuilder().ProgramChange(GeneralMidiProgram.Harpsichord).SetNoteLength(MusicalTimeSpan.Quarter) },
    { Voice.Bass, new PatternBuilder().ProgramChange(GeneralMidiProgram.Harpsichord).SetNoteLength(MusicalTimeSpan.Quarter) }
};

// Use pattern builders to add notes to the pattern for each voice...
foreach (var measure in composition.Measures)
{
    foreach (var beat in measure.Beats)
    {
        patternBuildersByVoice[Voice.Soprano].Note(beat.Chord[Voice.Soprano].Raw);
        patternBuildersByVoice[Voice.Alto].Note(beat.Chord[Voice.Alto].Raw);
        patternBuildersByVoice[Voice.Tenor].Note(beat.Chord[Voice.Tenor].Raw);
        patternBuildersByVoice[Voice.Bass].Note(beat.Chord[Voice.Bass].Raw);
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
