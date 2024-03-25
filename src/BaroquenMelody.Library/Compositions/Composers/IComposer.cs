using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <summary>
///     Represents a composer which can generate a <see cref="Composition"/>.
/// </summary>
internal interface IComposer
{
    /// <summary>
    ///    Composes a <see cref="Composition"/>.
    /// </summary>
    /// <returns> The composed <see cref="Composition"/>. </returns>
    Composition Compose();
}
