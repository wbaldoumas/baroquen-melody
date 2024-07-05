using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

/// <summary>
///     Represents a cleaner that removes conflicting ornamentations across two <see cref="BaroquenNote"/> objects.
/// </summary>
internal interface IOrnamentationCleaner
{
    /// <summary>
    ///     Removes conflicting ornamentations from two <see cref="BaroquenNote"/> objects.
    /// </summary>
    /// <param name="noteA">The first note to clean.</param>
    /// <param name="noteB">The second note to clean.</param>
    void Clean(BaroquenNote noteA, BaroquenNote noteB);
}
