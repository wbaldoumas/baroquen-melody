using BaroquenMelody.Library.Compositions.Domain;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

/// <summary>
///     A cleaner that does nothing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "No-op cleaner.")]
internal sealed class NoOpOrnamentationCleaner : IOrnamentationCleaner
{
    public void Clean(BaroquenNote noteA, BaroquenNote noteB)
    {
    }
}
