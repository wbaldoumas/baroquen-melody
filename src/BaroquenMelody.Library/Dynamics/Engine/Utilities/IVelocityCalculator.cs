using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using Melanchall.DryWetMidi.Common;

namespace BaroquenMelody.Library.Dynamics.Engine.Utilities;

/// <summary>
///     Calculates the velocity of the last preceding note of the given instrument.
/// </summary>
internal interface IVelocityCalculator
{
    /// <summary>
    ///     Gets the velocity of the last preceding note of the given instrument.
    /// </summary>
    /// <param name="precedingBeats">The preceding beats to get the velocity from.</param>
    /// <param name="instrument">The instrument to get the velocity for.</param>
    /// <returns>The velocity of the last preceding note of the given instrument.</returns>
    /// <exception cref="InvalidOperationException">Thrown when preceding beats is empty.</exception>
    SevenBitNumber GetPrecedingVelocity(FixedSizeList<Beat> precedingBeats, Instrument instrument);

    /// <summary>
    ///     Gets the velocities of the last preceding notes of the given instrument.
    /// </summary>
    /// <param name="precedingBeats">The preceding beats to get the velocities from.</param>
    /// <param name="instrument">The instrument to get the velocities for.</param>
    /// <param name="count">The number of notes to check.</param>
    /// <returns>The velocities of the last preceding notes of the given instrument.</returns>
    /// <exception cref="InvalidOperationException">Thrown when preceding beats is empty.</exception>
    IEnumerable<SevenBitNumber> GetPrecedingVelocities(FixedSizeList<Beat> precedingBeats, Instrument instrument, int count = 4);
}
