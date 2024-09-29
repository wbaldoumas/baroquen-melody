namespace BaroquenMelody.Library.Composers;

/// <summary>
///    A composer that generates Baroquen melodies.
/// </summary>
public interface IBaroquenMelodyComposer
{
    /// <summary>
    ///     Compose a Baroquen melody.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cooperatively cancel composition.</param>
    /// <returns>The composed Baroquen melody.</returns>
    BaroquenMelody Compose(CancellationToken cancellationToken);
}
