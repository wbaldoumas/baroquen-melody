using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;

namespace BaroquenMelody.Library.Compositions.Configurations;

/// <summary>
///    The voice configuration. Allowing for the configuration of the note range for a given voice.
/// </summary>
/// <param name="Voice"> The voice to be configured. </param>
/// <param name="MinNote"> The voice's minimum note value. </param>
/// <param name="MaxNote"> The voice's maximum note value. </param>
/// <param name="IsEnabled"> Whether the voice is enabled. </param>
/// <param name="Instrument"> The voice's instrument. </param>
public sealed record VoiceConfiguration(
    Voice Voice,
    Note MinNote,
    Note MaxNote,
    GeneralMidi2Program Instrument = GeneralMidi2Program.AcousticGrandPiano,
    bool IsEnabled = true)
{
    public bool IsNoteWithinVoiceRange(Note note) => MinNote.NoteNumber <= note.NoteNumber && note.NoteNumber <= MaxNote.NoteNumber;
}
