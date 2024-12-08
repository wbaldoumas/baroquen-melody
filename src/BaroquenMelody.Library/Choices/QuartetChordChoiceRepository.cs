using BaroquenMelody.Library.Choices.Extensions;
using BaroquenMelody.Library.Configurations;
using LazyCart;
using System.Numerics;

namespace BaroquenMelody.Library.Choices;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class QuartetChordChoiceRepository : IChordChoiceRepository
{
    public const int NumberOfInstruments = 4;

    private readonly LazyCartesianProduct<NoteChoice, NoteChoice, NoteChoice, NoteChoice> _noteChoices;

    public QuartetChordChoiceRepository(
        CompositionConfiguration configuration,
        INoteChoiceGenerator noteChoiceGenerator)
    {
        if (configuration.InstrumentConfigurations.Count != NumberOfInstruments)
        {
            throw new ArgumentException(
                "The composition configuration must contain exactly four instrument configurations.",
                nameof(configuration)
            );
        }

        var noteChoicesForInstruments = configuration.Instruments
            .Order()
            .Select(noteChoiceGenerator.GenerateNoteChoices)
            .ToList();

        _noteChoices = new LazyCartesianProduct<NoteChoice, NoteChoice, NoteChoice, NoteChoice>(
            [.. noteChoicesForInstruments[0]],
            [.. noteChoicesForInstruments[1]],
            [.. noteChoicesForInstruments[2]],
            [.. noteChoicesForInstruments[3]]
        );
    }

    public BigInteger Count => _noteChoices.Size;

    public ChordChoice GetChordChoice(BigInteger id) => _noteChoices[id].ToChordChoice();
}
