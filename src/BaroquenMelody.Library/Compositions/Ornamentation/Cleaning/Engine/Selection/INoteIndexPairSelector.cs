using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Selection;

/// <summary>
///     Selects the note indices to consider for cleaning ornamentations.
/// </summary>
internal interface INoteIndexPairSelector
{
    /// <summary>
    ///     Selects the note indices to consider for cleaning ornamentations.
    /// </summary>
    /// <param name="primaryOrnamentationType">The primary ornamentation type to consider for cleaning ornamentations.</param>
    /// <param name="secondaryOrnamentationType">The secondary ornamentation type to consider for cleaning ornamentations.</param>
    /// <returns>The note indices to consider for cleaning ornamentations.</returns>
    IEnumerable<NoteIndexPair> Select(OrnamentationType primaryOrnamentationType, OrnamentationType secondaryOrnamentationType);
}
