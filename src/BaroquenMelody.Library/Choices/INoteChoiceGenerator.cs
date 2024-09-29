using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Choices;

/// <summary>
///     Generates the possible note choices for the given instrument.
/// </summary>
internal interface INoteChoiceGenerator
{
    /// <summary>
    ///    Generates the possible note choices for the given instrument.
    /// </summary>
    /// <param name="instrument"> The instrument to generate note choices for. </param>
    /// <returns> The possible note choices for the given instrument. </returns>
    ISet<NoteChoice> GenerateNoteChoices(Instrument instrument);
}
