using BaroquenMelody.Library.Composition.Configurations;

namespace BaroquenMelody.Library.Composition.Choices;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class ChordChoiceRepositoryFactory : IChordChoiceRepositoryFactory
{
    private readonly INoteChoiceGenerator _noteChoiceGenerator;

    public ChordChoiceRepositoryFactory(INoteChoiceGenerator noteChoiceGenerator) =>
        _noteChoiceGenerator = noteChoiceGenerator;

    public IChordChoiceRepository Create(CompositionConfiguration compositionConfiguration) =>
        compositionConfiguration.VoiceConfigurations.Count switch
        {
            DuetChordChoiceRepository.NumberOfVoices => new DuetChordChoiceRepository(compositionConfiguration, _noteChoiceGenerator),
            TrioChordChoiceRepository.NumberOfVoices => new TrioChordChoiceRepository(compositionConfiguration, _noteChoiceGenerator),
            QuartetChordChoiceRepository.NumberOfVoices => new QuartetChordChoiceRepository(compositionConfiguration, _noteChoiceGenerator),
            _ => throw new ArgumentException(
                "The composition configuration must contain between two and four voice configurations.",
                nameof(compositionConfiguration)
            )
        };
}
