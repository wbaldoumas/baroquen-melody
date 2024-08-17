using BaroquenMelody.Library.Compositions.Configurations;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class ChordChoiceRepositoryFactory(INoteChoiceGenerator noteChoiceGenerator) : IChordChoiceRepositoryFactory
{
    public IChordChoiceRepository Create(CompositionConfiguration compositionConfiguration) => compositionConfiguration.Instruments.Count switch
    {
        DuetChordChoiceRepository.NumberOfInstruments => new DuetChordChoiceRepository(compositionConfiguration, noteChoiceGenerator),
        TrioChordChoiceRepository.NumberOfInstruments => new TrioChordChoiceRepository(compositionConfiguration, noteChoiceGenerator),
        QuartetChordChoiceRepository.NumberOfInstruments => new QuartetChordChoiceRepository(compositionConfiguration, noteChoiceGenerator),
        _ => throw new ArgumentException(
            "The composition configuration must contain between two and four instrument configurations.",
            nameof(compositionConfiguration)
        )
    };
}
