using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Ornamentation;

/// <summary>
///     Represents a piece of a composition that can be ornamented.
/// </summary>
/// <param name="Instrument">The instrument that the ornamentation is being applied to.</param>
/// <param name="PrecedingBeats">The beats preceding the current beat to be ornamented.</param>
/// <param name="CurrentBeat">The beat to be ornamented.</param>
/// <param name="NextBeat">The beat following the current beat to be ornamented.</param>
[ExcludeFromCodeCoverage(Justification = "Record with no logic.")]
internal sealed record OrnamentationItem(
    Instrument Instrument,
    IReadOnlyList<Beat> PrecedingBeats,
    Beat CurrentBeat,
    Beat? NextBeat
);
