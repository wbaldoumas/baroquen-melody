using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;

namespace BaroquenMelody.Library.Compositions.Contexts;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class ChordContextRepositoryFactory(INoteContextGenerator noteContextGenerator) : IChordContextRepositoryFactory
{
    public IChordContextRepository Create(CompositionConfiguration compositionConfiguration) => compositionConfiguration.VoiceConfigurations.Count switch
    {
        DuetChordChoiceRepository.NumberOfVoices => new DuetChordContextRepository(compositionConfiguration, noteContextGenerator),
        TrioChordChoiceRepository.NumberOfVoices => new TrioChordContextRepository(compositionConfiguration, noteContextGenerator),
        QuartetChordChoiceRepository.NumberOfVoices => new QuartetChordContextRepository(compositionConfiguration, noteContextGenerator),
        _ => throw new ArgumentException(
            "The composition configuration must contain between two and four voice configurations.",
            nameof(compositionConfiguration)
        )
    };
}
