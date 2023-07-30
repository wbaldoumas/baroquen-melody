using BaroquenMelody.Library.Composition.Configurations;

namespace BaroquenMelody.Library.Composition.Contexts;

/// <summary>
///     A factory for creating <see cref="IChordContextRepository"/>s.
/// </summary>
internal interface IChordContextRepositoryFactory
{
    /// <summary>
    ///     Creates a <see cref="IChordContextRepository"/> for the given <see cref="CompositionConfiguration"/>. This is needed
    ///     because the specific <see cref="IChordContextRepository"/> to use depends on how many voices are in the composition.
    /// </summary>
    /// <param name="compositionConfiguration"> The <see cref="CompositionConfiguration"/> to create the <see cref="IChordContextRepository"/> for. </param>
    /// <returns> The created <see cref="IChordContextRepository"/>. </returns>
    IChordContextRepository Create(CompositionConfiguration compositionConfiguration);
}
