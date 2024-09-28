using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Selection.Strategies;

/// <summary>
///     A strategy for selecting a note to clean ornamentations for out of the two notes provided.
/// </summary>
internal interface IOrnamentationCleaningSelectorStrategy
{
    /// <summary>
    ///     Selects a note to clean ornamentations for out of the two notes provided.
    ///     If null is returned, the selector could not determine which note to clean.
    /// </summary>
    /// <param name="primaryNote">The primary note to consider for ornamentation cleaning.</param>
    /// <param name="secondaryNote">The secondary note to consider for ornamentation cleaning.</param>
    /// <returns>The note to clean, or null if the selector could not determine which note to clean.</returns>
    BaroquenNote? Select(BaroquenNote primaryNote, BaroquenNote secondaryNote);
}
