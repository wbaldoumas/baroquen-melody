namespace BaroquenMelody.Library.Compositions.Composers;

/// <summary>
///    A composer that generates Baroquen melodies.
/// </summary>
public interface IBaroquenMelodyComposer
{
    /// <summary>
    ///     Compose a Baroquen melody.
    /// </summary>
    /// <returns>The composed Baroquen melody.</returns>
    BaroquenMelody Compose();
}
