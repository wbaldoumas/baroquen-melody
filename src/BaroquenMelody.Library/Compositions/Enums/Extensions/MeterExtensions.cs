using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Compositions.Enums.Extensions;

/// <summary>
///     A place for extensions for <see cref="Meter"/>.
/// </summary>
public static class MeterExtensions
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
        Meter.ThreeFour => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(meter), meter, "Meter not supported.")
    };

    /// <summary>
    ///    Gets the default <see cref="MusicalTimeSpan"/> for the given <see cref="Meter"/>.
    /// </summary>
    /// <param name="meter">The <see cref="Meter"/> to get the default <see cref="MusicalTimeSpan"/> for.</param>
    /// <returns>The default <see cref="MusicalTimeSpan"/> for the given <see cref="Meter"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown if the given <see cref="Meter"/> is not supported. </exception>
    public static MusicalTimeSpan DefaultMusicalTimeSpan(this Meter meter) => meter switch
    {
        Meter.FourFour => MusicalTimeSpan.Half,
        Meter.ThreeFour => MusicalTimeSpan.Half.Dotted(1),
        _ => throw new ArgumentOutOfRangeException(nameof(meter), meter, "Meter not supported.")
    };
}
