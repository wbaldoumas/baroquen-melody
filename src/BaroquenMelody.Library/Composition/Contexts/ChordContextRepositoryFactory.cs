using BaroquenMelody.Library.Composition.Choices;
using BaroquenMelody.Library.Composition.Configurations;

namespace BaroquenMelody.Library.Composition.Contexts;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class ChordContextRepositoryFactory : IChordContextRepositoryFactory
{
    private readonly INoteContextGenerator _noteContextGenerator;

    public ChordContextRepositoryFactory(INoteContextGenerator noteContextGenerator) =>
        _noteContextGenerator = noteContextGenerator;

    public IChordContextRepository Create(CompositionConfiguration compositionConfiguration) =>
        compositionConfiguration.VoiceConfigurations.Count switch
        {
            DuetChordChoiceRepository.NumberOfVoices => new DuetChordContextRepository(
                compositionConfiguration,
                _noteContextGenerator
            ),
            TrioChordChoiceRepository.NumberOfVoices => new TrioChordContextRepository(
                compositionConfiguration,
                _noteContextGenerator
            ),
            QuartetChordChoiceRepository.NumberOfVoices => new QuartetChordContextRepository(
                compositionConfiguration,
                _noteContextGenerator
            ),
            _ => throw new ArgumentException(
                "The composition configuration must contain between two and four voice configurations.",
                nameof(compositionConfiguration)
            )
        };
}
