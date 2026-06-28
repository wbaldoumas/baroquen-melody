using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Motifs;

/// <summary>
///     Produces a developed (transformed) restatement of a recurring phrase, or signals that it should be repeated verbatim.
/// </summary>
internal interface IMotifDeveloper
{
    /// <summary>
    ///     Attempts to develop the given phrase by transforming one voice's cataloged motif and substituting it back onto
    ///     the phrase's beat grid. All randomness flows through the injected providers, so the result is deterministic under
    ///     a seed; when motivic development is disabled the method returns without drawing any randomness.
    /// </summary>
    /// <param name="motifBank">The per-voice catalog of the theme's motifs.</param>
    /// <param name="phrase">The phrase about to be restated.</param>
    /// <returns>A developed clone of the phrase, or <see langword="null"/> to fall back to a verbatim restatement.</returns>
    List<Measure>? TryDevelop(MotifBank motifBank, IReadOnlyList<Measure> phrase);
}
