using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Choices;

/// <summary>
///     Generates the possible note choices for the given instrument.
/// </summary>
internal interface INoteChoiceGenerator
{
    /// <summary>
    ///    Generates the possible note choices for the given instrument, in a canonical order. The order defines the
    ///    chord-choice index space shared by the choice repositories and the forward-checking enumerator, so it must
    ///    be identical in every process for seeded composition to be reproducible.
    /// </summary>
    /// <param name="instrument"> The instrument to generate note choices for. </param>
    /// <returns> The distinct possible note choices for the given instrument, in a canonical order. </returns>
    IReadOnlyList<NoteChoice> GenerateNoteChoices(Instrument instrument);
}
