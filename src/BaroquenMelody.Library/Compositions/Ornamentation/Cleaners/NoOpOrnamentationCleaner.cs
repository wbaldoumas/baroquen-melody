using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

/// <inheritdoc cref="IOrnamentationCleaner"/>
internal sealed class NoOpOrnamentationCleaner : IOrnamentationCleaner
{
    public void Clean(BaroquenNote noteA, BaroquenNote noteB)
    {
    }
}
