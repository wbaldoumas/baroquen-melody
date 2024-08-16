using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;

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
    Note MinNote,
    Note MaxNote,
    GeneralMidi2Program MidiProgram = GeneralMidi2Program.AcousticGrandPiano,
    bool IsEnabled = true)
{
    public bool IsNoteWithinInstrumentRange(Note note) => MinNote.NoteNumber <= note.NoteNumber && note.NoteNumber <= MaxNote.NoteNumber;
}
