using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

/// <summary>
///     Represents a factory that allows for retrieval of <see cref="IOrnamentationCleaner"/> objects.
/// </summary>
internal interface IOrnamentationCleanerFactory
{
    /// <summary>
    ///     Get the <see cref="IOrnamentationCleaner"/> for the given ornamentation types.
    /// </summary>
    /// <param name="ornamentationTypeA">The first ornamentation type.</param>
    /// <param name="ornamentationTypeB">The second ornamentation type.</param>
    /// <returns>The <see cref="IOrnamentationCleaner"/> for the given ornamentation types.</returns>
    IOrnamentationCleaner Get(OrnamentationType ornamentationTypeA, OrnamentationType ornamentationTypeB);
}
