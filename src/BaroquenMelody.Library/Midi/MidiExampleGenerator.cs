using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Midi.Extensions;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Enums;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Standards;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Midi;

internal sealed class MidiExampleGenerator(IOrnamentationProcessorFactory ornamentationProcessorFactory) : IMidiExampleGenerator
{
    private const int DefaultTempo = 120;

    public MidiFile GenerateExampleNoteMidiFile(GeneralMidi2Program midiProgram, Note note)
    {
        var patternBuilder = new PatternBuilder().ProgramChange(midiProgram);
        var baroquenNote = new BaroquenNote(Instrument.One, note, MusicalTimeSpan.Quarter);

        patternBuilder.AddNote(baroquenNote).AddRest(MusicalTimeSpan.Quarter);

        var tempoMap = TempoMap.Create(Tempo.FromBeatsPerMinute(DefaultTempo));
        var chunks = patternBuilder.Build().ToTrackChunk(tempoMap, FourBitNumber.MinValue);
        var midiFile = new MidiFile(chunks);

        midiFile.ReplaceTempoMap(tempoMap);

        return midiFile;
    }

    public MidiFile GenerateExampleOrnamentationMidiFile(OrnamentationType ornamentationType, CompositionConfiguration compositionConfiguration)
    {
        var ornamentationProcessor = ornamentationProcessorFactory.Create(ornamentationType, compositionConfiguration, GetInterval(ornamentationType));
        var note = GetNote(ornamentationType, compositionConfiguration);
        var beat = new Beat(new BaroquenChord([note]));
        var nextNote = GetNextNote(ornamentationType, compositionConfiguration);
        var nextBeat = new Beat(new BaroquenChord([nextNote]));
        var ornamentationItem = new OrnamentationItem(note.Instrument, [], beat, nextBeat);

        ornamentationProcessor.Process(ornamentationItem);

        var patternBuilder = new PatternBuilder().ProgramChange(GeneralMidi2Program.AcousticGrandPiano);

        patternBuilder.AddNote(note);
        patternBuilder.AddNote(nextNote);
        patternBuilder.AddRest(MusicalTimeSpan.Quarter);

        var tempoMap = TempoMap.Create(Tempo.FromBeatsPerMinute(DefaultTempo));
        var chunks = patternBuilder.Build().ToTrackChunk(tempoMap, FourBitNumber.MinValue);
        var midiFile = new MidiFile(chunks);

        midiFile.ReplaceTempoMap(tempoMap);

        return midiFile;
    }

    private static int GetInterval(OrnamentationType ornamentationType) => ornamentationType switch
    {
        OrnamentationType.DecorateInterval => -4,
        OrnamentationType.Pedal => PedalProcessor.RootPedalInterval,
        _ => 0
    };

    private static BaroquenNote GetNote(OrnamentationType ornamentationType, CompositionConfiguration compositionConfiguration) => ornamentationType switch
    {
        OrnamentationType.DecorateInterval => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Supertonic, 4), compositionConfiguration.DefaultNoteTimeSpan),
        _ => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Tonic, 4), compositionConfiguration.DefaultNoteTimeSpan)
    };

    private static BaroquenNote GetNextNote(OrnamentationType ornamentationType, CompositionConfiguration compositionConfiguration) => ornamentationType switch
    {
        OrnamentationType.PassingTone => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Mediant, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.Run => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Dominant, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.DelayedPassingTone => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Mediant, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.Turn => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Mediant, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.InvertedTurn => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Supertonic, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.DelayedRun => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Submediant, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.DoubleTurn => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Dominant, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.DoubleInvertedTurn => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Mediant, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.DoublePassingTone => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Subdominant, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.DelayedDoublePassingTone => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Subdominant, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.DecorateInterval => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Tonic, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.DoubleRun => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Submediant, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.Pedal => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Submediant, 3), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.Mordent => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Tonic, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.RepeatedNote => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Tonic, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.DelayedRepeatedNote => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Tonic, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.NeighborTone => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Tonic, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.DelayedNeighborTone => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Tonic, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.Pickup => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Mediant, 4), compositionConfiguration.DefaultNoteTimeSpan),
        OrnamentationType.DelayedPickup => new BaroquenNote(Instrument.One, Note.Get(compositionConfiguration.Scale.Mediant, 4), compositionConfiguration.DefaultNoteTimeSpan),
        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, "Ornamentation type not supported.")
    };
}
