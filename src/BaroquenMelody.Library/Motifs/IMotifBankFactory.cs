using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Motifs;

/// <summary>
///     Catalogs a theme's per-voice motifs into a <see cref="MotifBank"/>.
/// </summary>
internal interface IMotifBankFactory
{
    /// <summary>
    ///     Builds a <see cref="MotifBank"/> from the theme's exposition: one <see cref="AnchoredMotif"/> per voice,
    ///     taken from the first exposition measure that voice appears in.
    /// </summary>
    /// <param name="theme">The composed theme.</param>
    /// <returns>The cataloged motifs.</returns>
    MotifBank Create(BaroquenTheme theme);
}
