namespace BaroquenMelody.Library.Compositions.Enums.Extensions;

/// <summary>
///     A place for extensions for <see cref="Meter"/>.
/// </summary>
internal static class MeterExtensions
{
    /// <summary>
    ///    Gets the number of beats per measure for the given <see cref="Meter"/>.
    /// </summary>
    /// <param name="meter"> The <see cref="Meter"/> to get the number of beats per measure for. </param>
    /// <returns> The number of beats per measure for the given <see cref="Meter"/>. </returns>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown if the given <see cref="Meter"/> is not supported. </exception>
    public static int BeatsPerMeasure(this Meter meter) => meter switch
    {
        Meter.FourFour => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(meter), meter, null)
    };
}
