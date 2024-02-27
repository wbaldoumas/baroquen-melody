using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <summary>
///    Represents a choice of note motion and scale step change for a given voice to move to the next note.
/// </summary>
/// <param name="Voice"> The voice associated with the note choice. </param>
/// <param name="Motion"> The motion which will be used to move to the next note. </param>
/// <param name="ScaleStepChange"> The change in scale steps which will be used to move to the next note. </param>
internal sealed record NoteChoice(
    Voice Voice,
    NoteMotion Motion,
    byte ScaleStepChange
);
