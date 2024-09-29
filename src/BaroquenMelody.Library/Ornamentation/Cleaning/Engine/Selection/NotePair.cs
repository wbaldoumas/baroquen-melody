using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection;

/// <summary>
///     A pair of notes to consider for ornamentation cleaning.
/// </summary>
/// <param name="PrimaryNote">The primary note to consider for ornamentation cleaning.</param>
/// <param name="SecondaryNote">The secondary note to consider for ornamentation cleaning.</param>
internal sealed record NotePair(BaroquenNote PrimaryNote, BaroquenNote SecondaryNote);
