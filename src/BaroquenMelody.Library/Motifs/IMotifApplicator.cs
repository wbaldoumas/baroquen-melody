using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Motifs;

/// <summary>
///     Reconstructs a concrete voice line from an <see cref="AnchoredMotif"/>.
/// </summary>
internal interface IMotifApplicator
{
    /// <summary>
    ///     Reconstructs a concrete principal voice line from an anchored motif. Each note is an in-scale note; one whose
    ///     ideal pitch falls outside the instrument's playable range clamps to the nearest boundary scale note within it.
    ///     Returned notes carry no ornamentation; each note's duration comes verbatim from its gesture.
    /// </summary>
    /// <param name="anchoredMotif">The motif plus its anchor and instrument.</param>
    /// <returns>The reconstructed principal voice line, clamped into the instrument's playable range.</returns>
    IReadOnlyList<BaroquenNote> Apply(AnchoredMotif anchoredMotif);
}
