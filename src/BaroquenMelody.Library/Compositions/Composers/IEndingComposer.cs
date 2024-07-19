using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <summary>
///     Represents a composer that can compose an ending for a <see cref="Composition"/>.
/// </summary>
internal interface IEndingComposer
{
    /// <summary>
    ///     Compose the ending for the given <see cref="Composition"/> using the given <see cref="BaroquenTheme"/>.
    /// </summary>
    /// <param name="composition">The composition to complete.</param>
    /// <param name="theme">The theme to try using to complete the composition.</param>
    /// <returns>The completed composition.</returns>
    Composition Compose(Composition composition, BaroquenTheme theme);
}
