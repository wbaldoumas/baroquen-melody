using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Dynamics;

/// <summary>
///     Represents a unit of work for applying dynamics to a composition.
/// </summary>
internal sealed class DynamicsApplicationItem
{
    /// <summary>
    ///     The instrument to apply dynamics to.
    /// </summary>
    public required Instrument Instrument { get; init; }

    /// <summary>
    ///     The instruments that have already had dynamics applied to them, impacting the current instruments dynamics.
    /// </summary>
    public required HashSet<Instrument> ProcessedInstruments { get; init; }

    /// <summary>
    ///     The context of the composition, used to determine the dynamics of the current instrument.
    /// </summary>
    public required FixedSizeList<Beat> PrecedingBeats { get; init; }

    /// <summary>
    ///     The current beat to apply dynamics to.
    /// </summary>
    public required Beat CurrentBeat { get; init; }

    /// <summary>
    ///     The next beat to apply dynamics to.
    /// </summary>
    public Beat? NextBeat { get; init; }

    /// <summary>
    ///     Whether the current beat has already had dynamics applied to it.
    /// </summary>
    public bool HasProcessedCurrentBeat { get; set; }

    /// <summary>
    ///     Whether the next beat has already had dynamics applied to it.
    /// </summary>
    public bool HasProcessedNextBeat { get; set; }
}
