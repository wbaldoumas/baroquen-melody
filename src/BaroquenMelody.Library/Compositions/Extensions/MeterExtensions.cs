using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Extensions;

internal static class MeterExtensions
{
    public static int BeatsPerMeasure(this Meter meter) => meter switch
    {
        Meter.FourFour => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(meter), meter, null)
    };
}
