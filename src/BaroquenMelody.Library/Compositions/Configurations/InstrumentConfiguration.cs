using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Infrastructure.Serialization.JsonConverters;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using System.Text.Json.Serialization;

namespace BaroquenMelody.Library.Compositions.Configurations;

/// <summary>
///    The instrument configuration. Allowing for the configuration of the note range for a given instrument.
/// </summary>
/// <param name="Instrument"> The instrument to be configured. </param>
/// <param name="MinNote"> The instrument's minimum note value. </param>
/// <param name="MaxNote"> The instrument's maximum note value. </param>
/// <param name="IsEnabled"> Whether the instrument is enabled. </param>
/// <param name="MidiProgram"> The instrument's midi program. </param>
public sealed record InstrumentConfiguration(
    Instrument Instrument,
    [property: JsonConverter(typeof(NoteJsonConverter))] Note MinNote,
    [property: JsonConverter(typeof(NoteJsonConverter))] Note MaxNote,
    GeneralMidi2Program MidiProgram = GeneralMidi2Program.AcousticGrandPiano,
    bool IsEnabled = true)
{
    public bool IsNoteWithinInstrumentRange(Note note) => MinNote.NoteNumber <= note.NoteNumber && note.NoteNumber <= MaxNote.NoteNumber;

    public static readonly IDictionary<Instrument, InstrumentConfiguration> DefaultConfigurations = new Dictionary<Instrument, InstrumentConfiguration>
    {
        { Instrument.One, new InstrumentConfiguration(Instrument.One, Notes.C5, Notes.E6) },
        { Instrument.Two, new InstrumentConfiguration(Instrument.Two, Notes.G3, Notes.B4) },
        { Instrument.Three, new InstrumentConfiguration(Instrument.Three, Notes.D3, Notes.F4) },
        { Instrument.Four, new InstrumentConfiguration(Instrument.Four, Notes.C2, Notes.E3, IsEnabled: false) }
    };
}
