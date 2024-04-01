using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

/// <summary>
///    Represents a calculator that can calculate the time span of primary notes and their ornamentations.
/// </summary>
internal interface IMusicalTimeSpanCalculator
{
    /// <summary>
    ///     Calculates the time span of the primary note based on the ornamentation type and the meter.
    /// </summary>
    /// <param name="ornamentationType">The type of ornamentation to calculate the time span for.</param>
    /// <param name="meter">The meter of the composition. This can impact the time span, especially for non-common meters.</param>
    /// <returns>The time span of the primary note.</returns>
    MusicalTimeSpan CalculatePrimaryNoteTimeSpan(OrnamentationType ornamentationType, Meter meter);

    /// <summary>
    ///     Calculates the time span of a note based on the ornamentation type and the meter.
    /// </summary>
    /// <param name="ornamentationType">The type of ornamentation to calculate the time span for.</param>
    /// <param name="meter">The meter of the composition. This can impact the time span, especially for non-common meters.</param>
    /// <returns>The time span of the ornamentation.</returns>
    MusicalTimeSpan CalculateOrnamentationTimeSpan(OrnamentationType ornamentationType, Meter meter);
}
