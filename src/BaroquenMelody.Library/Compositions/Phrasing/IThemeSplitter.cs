using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Phrasing;

/// <summary>
///     Represents something that can split a <see cref="BaroquenTheme"/> into smaller parts.
/// </summary>
internal interface IThemeSplitter
{
    /// <summary>
    ///     Splits the given <paramref name="theme"/> into phrases.
    /// </summary>
    /// <param name="theme">The theme to split into phrases.</param>
    /// <returns>A list of phrases generated from the given theme.</returns>
    List<RepeatedPhrase> SplitThemeIntoPhrases(BaroquenTheme theme);
}
