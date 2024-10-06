using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Midi.Extensions;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Standards;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Midi;

public sealed class MidiExampleGenerator : IMidiExampleGenerator
{
    public MidiFile GenerateExampleNoteMidiFile(GeneralMidi2Program midiProgram, Note note)
    {
        var patternBuilder = new PatternBuilder().ProgramChange(midiProgram);

        var baroquenNote = new BaroquenNote(Instrument.One, note, MusicalTimeSpan.Quarter)
        {
            Velocity = new SevenBitNumber(100)
        };

        patternBuilder.AddNote(baroquenNote).AddRest(MusicalTimeSpan.Quarter);

        var tempoMap = TempoMap.Create(Tempo.FromBeatsPerMinute(120));
        var chunks = patternBuilder.Build().ToTrackChunk(tempoMap, FourBitNumber.Values[0]);
        var midiFile = new MidiFile(chunks);

        midiFile.ReplaceTempoMap(tempoMap);

        return midiFile;
    }
}
