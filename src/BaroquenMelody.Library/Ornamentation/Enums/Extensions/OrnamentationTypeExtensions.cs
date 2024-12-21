using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Enums.Extensions;

internal static class OrnamentationTypeExtensions
{
    private static readonly FrozenDictionary<OrnamentationType, int> OrnamentationCountsByType = new Dictionary<OrnamentationType, int>
    {
        { OrnamentationType.DecorateInterval, 3 },
        { OrnamentationType.DelayedRun, 4 },
        { OrnamentationType.DoublePassingTone, 2 },
        { OrnamentationType.DelayedDoublePassingTone, 2 },
        { OrnamentationType.DoubleRun, 7 },
        { OrnamentationType.DoubleTurn, 7 },
        { OrnamentationType.DoubleInvertedTurn, 7 },
        { OrnamentationType.InvertedTurn, 3 },
        { OrnamentationType.Mordent, 2 },
        { OrnamentationType.NeighborTone, 1 },
        { OrnamentationType.DelayedNeighborTone, 1 },
        { OrnamentationType.PassingTone, 1 },
        { OrnamentationType.DelayedPassingTone, 1 },
        { OrnamentationType.Pedal, 3 },
        { OrnamentationType.RepeatedNote, 1 },
        { OrnamentationType.DelayedRepeatedNote, 1 },
        { OrnamentationType.Run, 3 },
        { OrnamentationType.Turn, 3 },
        { OrnamentationType.Pickup, 1 },
        { OrnamentationType.DelayedPickup, 1 },
        { OrnamentationType.DoublePickup, 2 },
        { OrnamentationType.None, 0 },
        { OrnamentationType.Sustain, 0 },
        { OrnamentationType.MidSustain, 0 },
        { OrnamentationType.Rest, 0 }
    }.ToFrozenDictionary();

    public static int OrnamentationCount(this OrnamentationType ornamentationType) => OrnamentationCountsByType.TryGetValue(ornamentationType, out var ornamentationCount)
        ? ornamentationCount
        : throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, "Ornamentation type not found.");
}
