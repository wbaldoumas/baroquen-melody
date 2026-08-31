using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Choices;

/// <summary>
///     Generates the possible note choices for the given instrument.
/// </summary>
internal interface INoteChoiceGenerator
{
    /// <summary>
    ///    Generates the possible note choices for the given instrument. The order is part of the contract: the
    ///    chord-choice repositories and the forward-checking enumerator index into it, so it decides which candidate
    ///    each seeded draw lands on and must be a pure function of the configuration, never of hash-bucket layout.
    /// </summary>
    /// <param name="instrument"> The instrument to generate note choices for. </param>
    /// <returns> The possible note choices for the given instrument, in generation order. </returns>
    IReadOnlyList<NoteChoice> GenerateNoteChoices(Instrument instrument);
}
