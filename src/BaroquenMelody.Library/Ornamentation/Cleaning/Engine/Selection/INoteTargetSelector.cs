using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection;

/// <summary>
///     Selects a target for ornamentation cleaning from a given <see cref="OrnamentationCleaningItem" />.
/// </summary>
internal interface INoteTargetSelector
{
    /// <summary>
    ///     Selects a target for ornamentation cleaning from a given <see cref="OrnamentationCleaningItem" />.
    /// </summary>
    /// <param name="item">The item to select a target note to clean ornamentations from.</param>
    /// <returns>The target note to clean.</returns>
    BaroquenNote Select(OrnamentationCleaningItem item);
}
