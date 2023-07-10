using BaroquenMelody.Library.Composition.Enums;

namespace BaroquenMelody.Library.Composition.Choices;

/// <summary>
///    Represents a choice of note motion and pitch change for a given voice to arrive at the next note.
/// </summary>
/// <param name="Voice"> The voice associated with the note choice. </param>
/// <param name="Motion"> The motion which will be used to arrive at the next note. </param>
/// <param name="PitchChange"> The amount of pitch change which will be used to arrive at the next note. </param>
internal sealed record NoteChoice(
    Voice Voice,
    NoteMotion Motion,
    byte PitchChange
);
