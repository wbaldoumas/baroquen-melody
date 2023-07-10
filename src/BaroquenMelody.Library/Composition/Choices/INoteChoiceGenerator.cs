using BaroquenMelody.Library.Composition.Enums;

namespace BaroquenMelody.Library.Composition.Choices;

/// <summary>
///     Generates the possible note choices for the given voice.
/// </summary>
internal interface INoteChoiceGenerator
{
    /// <summary>
    ///    Generates the possible note choices for the given voice.
    /// </summary>
    /// <param name="voice"> The voice to generate note choices for. </param>
    /// <returns> The possible note choices for the given voice. </returns>
    ISet<NoteChoice> GenerateNoteChoices(Voice voice);
}
