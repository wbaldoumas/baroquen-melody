using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Enums.Extensions;

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
        Meter.ThreeFour => 4,
        Meter.FiveEight => 4,
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
        Meter.FiveEight => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,
        _ => throw new ArgumentOutOfRangeException(nameof(meter), meter, "Meter not supported.")
    };

    /// <summary>
    ///     Gets the <see cref="Meter"/> as a human-readable string.
    /// </summary>
    /// <param name="meter">The <see cref="Meter"/> to get as a human-readable string.</param>
    /// <returns>The <see cref="Meter"/> as a human-readable string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the given <see cref="Meter"/> is not supported.</exception>
    public static string AsString(this Meter meter) => meter switch
    {
        Meter.FourFour => "4/4",
        Meter.ThreeFour => "3/4",
        Meter.FiveEight => "5/8",
        _ => throw new ArgumentOutOfRangeException(nameof(meter), meter, "Meter not supported.")
    };

    /// <summary>
    ///     Converts the <see cref="Meter"/> to a <see cref="TimeSignature"/>.
    /// </summary>
    /// <param name="meter">The source <see cref="Meter"/>.</param>
    /// <returns>The <see cref="Meter"/> as a <see cref="TimeSignature"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the given <see cref="Meter"/> is not supported.</exception>
    public static TimeSignature ToTimeSignature(this Meter meter) => meter switch
    {
        Meter.FourFour => new TimeSignature(4, 4),
        Meter.ThreeFour => new TimeSignature(3, 4),
        Meter.FiveEight => new TimeSignature(5, 8),
        _ => throw new ArgumentOutOfRangeException(nameof(meter), meter, "Meter not supported.")
    };
}
