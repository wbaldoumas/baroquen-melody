using BaroquenMelody.Library.Choices.Extensions;
using BaroquenMelody.Library.Configurations;
using LazyCart;
using System.Numerics;

namespace BaroquenMelody.Library.Choices;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class DuetChordChoiceRepository : IChordChoiceRepository
{
    public const int NumberOfInstruments = 2;

    private readonly LazyCartesianProduct<NoteChoice, NoteChoice> _noteChoices;

    public DuetChordChoiceRepository(CompositionConfiguration configuration, INoteChoiceGenerator noteChoiceGenerator)
    {
        if (configuration.Instruments.Count != NumberOfInstruments)
        {
            throw new ArgumentException(
                "The composition configuration must contain exactly two instrument configurations.",
                nameof(configuration)
            );
        }

        var noteChoicesForInstruments = configuration.Instruments
            .Order()
            .Select(noteChoiceGenerator.GenerateNoteChoices)
            .ToList();

        _noteChoices = new LazyCartesianProduct<NoteChoice, NoteChoice>(
            [.. noteChoicesForInstruments[0]],
            [.. noteChoicesForInstruments[1]]
        );
    }

    public BigInteger Count => _noteChoices.Size;

    public ChordChoice GetChordChoice(BigInteger id) => _noteChoices[id].ToChordChoice();
}
