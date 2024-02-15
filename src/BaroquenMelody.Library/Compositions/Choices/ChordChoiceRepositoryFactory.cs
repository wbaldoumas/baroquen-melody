using BaroquenMelody.Library.Compositions.Configurations;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class ChordChoiceRepositoryFactory(INoteChoiceGenerator noteChoiceGenerator) : IChordChoiceRepositoryFactory
{
    public IChordChoiceRepository Create(CompositionConfiguration compositionConfiguration) =>
        compositionConfiguration.VoiceConfigurations.Count switch
        {
            DuetChordChoiceRepository.NumberOfVoices => new DuetChordChoiceRepository(compositionConfiguration, noteChoiceGenerator),
            TrioChordChoiceRepository.NumberOfVoices => new TrioChordChoiceRepository(compositionConfiguration, noteChoiceGenerator),
            QuartetChordChoiceRepository.NumberOfVoices => new QuartetChordChoiceRepository(compositionConfiguration, noteChoiceGenerator),
            _ => throw new ArgumentException(
                "The composition configuration must contain between two and four voice configurations.",
                nameof(compositionConfiguration)
            )
        };
}
