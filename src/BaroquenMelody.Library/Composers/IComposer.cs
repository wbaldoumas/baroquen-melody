using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Composers;

/// <summary>
///     Represents a composer which can generate a <see cref="Composition"/>.
/// </summary>
internal interface IComposer
{
    /// <summary>
    ///    Composes a <see cref="Composition"/>.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cooperatively cancel composition.</param>
    /// <returns> The composed <see cref="Composition"/>. </returns>
    Composition Compose(CancellationToken cancellationToken);
}
