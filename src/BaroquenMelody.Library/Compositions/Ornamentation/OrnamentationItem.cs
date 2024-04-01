using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Compositions.Ornamentation;

/// <summary>
///     Represents a piece of a composition that can be ornamented.
/// </summary>
/// <param name="Voice">The voice that the ornamentation is being applied to.</param>
/// <param name="PrecedingBeats">The beats preceding the current beat to be ornamented.</param>
/// <param name="CurrentBeat">The beat to be ornamented.</param>
/// <param name="NextBeat">The beat following the current beat to be ornamented.</param>
[ExcludeFromCodeCoverage(Justification = "Record with no logic.")]
internal sealed record OrnamentationItem(
    Voice Voice,
    IReadOnlyList<Beat> PrecedingBeats,
    Beat CurrentBeat,
    Beat? NextBeat
);
