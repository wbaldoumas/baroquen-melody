using BaroquenMelody.Library.Compositions.Configurations;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <summary>
///     A factory for creating <see cref="IChordChoiceRepository"/>s.
/// </summary>
internal interface IChordChoiceRepositoryFactory
{
    /// <summary>
    ///     Creates a <see cref="IChordChoiceRepository"/> for the given <see cref="CompositionConfiguration"/>. This is needed
    ///     because the specific <see cref="IChordChoiceRepository"/> implementation to use depends on how many voices are in the composition.
    /// </summary>
    /// <param name="compositionConfiguration"> The <see cref="CompositionConfiguration"/> to create the <see cref="IChordChoiceRepository"/> for. </param>
    /// <returns> The created <see cref="IChordChoiceRepository"/>. </returns>
    IChordChoiceRepository Create(CompositionConfiguration compositionConfiguration);
}
