using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Configurations;

/// <summary>
///    The voice configuration. Allowing for the configuration of the note range for a given voice.
/// </summary>
/// <param name="Voice"> The voice to be configured. </param>
/// <param name="MinNote"> The voice's minimum note value. </param>
/// <param name="MaxNote"> The voice's maximum note value. </param>
internal sealed record VoiceConfiguration(
    Voice Voice,
    Note MinNote,
    Note MaxNote)
{
    public bool IsNoteWithinVoiceRange(Note note) => note.NoteNumber >= MinNote.NoteNumber &&
                                                     note.NoteNumber <= MaxNote.NoteNumber;
}
