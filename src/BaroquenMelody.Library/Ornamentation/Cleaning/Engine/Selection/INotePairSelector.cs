namespace BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection;

/// <summary>
///     Selects a <see cref="NotePair" /> from a <see cref="OrnamentationCleaningItem" />.
/// </summary>
internal interface INotePairSelector
{
    /// <summary>
    ///     Selects a <see cref="NotePair" /> from a <see cref="OrnamentationCleaningItem" />.
    /// </summary>
    /// <param name="item">The item to select the primary and secondary note pair to consider for ornamentation cleaning from.</param>
    /// <returns>The primary and secondary note pair to consider for ornamentation cleaning.</returns>
    NotePair Select(OrnamentationCleaningItem item);
}
