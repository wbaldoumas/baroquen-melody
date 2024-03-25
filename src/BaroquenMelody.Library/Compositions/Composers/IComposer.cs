using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <summary>
///     Represents a composer which can generate a <see cref="BaroquenComposition"/>.
/// </summary>
internal interface IComposer
{
    /// <summary>
    ///    Composes a <see cref="BaroquenComposition"/>.
    /// </summary>
    /// <returns> The composed <see cref="BaroquenComposition"/>. </returns>
    BaroquenComposition Compose();
}
