using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Motifs;

/// <summary>
///     A <see cref="Motif"/> bound to the apply-time context needed to reconstruct concrete pitches: the absolute
///     scale index of its head note and the instrument that will sound it. Bundling them stops callers from
///     re-anchoring a purely relative motif against a mismatched pitch or voice. Develop in place with
///     <c>anchored with { Motif = MotifTransformations.Invert(anchored.Motif) }</c>.
/// </summary>
/// <remarks>
///     Equality is inherited from <see cref="Motif"/> (reference-based <c>Gestures</c> comparison), so an
///     <see cref="AnchoredMotif"/> must NEVER be used as a dictionary/set key; key by <see cref="Instrument"/> instead.
/// </remarks>
/// <param name="Motif">The voice-agnostic motif.</param>
/// <param name="AnchorScaleIndex">The absolute scale index its head note lands on (<c>scale.IndexOf(firstNote)</c>).</param>
/// <param name="Instrument">The instrument that will sound the reconstructed line.</param>
internal sealed record AnchoredMotif(Motif Motif, int AnchorScaleIndex, Instrument Instrument);
