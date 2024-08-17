using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <summary>
///    Represents a composer which can generate a <see cref="BaroquenTheme"/>.
/// </summary>
internal interface IThemeComposer
{
    /// <summary>
    ///     Composes a <see cref="BaroquenTheme"/>. When possible, the exposition of the composition
    ///     is the opening subject of a fugue. If a suitable fugue subject cannot be found, then the
    ///     composition begins with all four instruments sounding at once.
    /// </summary>
    /// <returns>A <see cref="BaroquenTheme"/>.</returns>
    BaroquenTheme Compose();
}
