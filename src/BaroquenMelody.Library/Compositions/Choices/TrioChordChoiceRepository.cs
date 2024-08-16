using BaroquenMelody.Library.Compositions.Choices.Extensions;
using BaroquenMelody.Library.Compositions.Configurations;
using LazyCart;
using System.Numerics;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class TrioChordChoiceRepository : IChordChoiceRepository
{
    public const int NumberOfInstruments = 3;

    private readonly LazyCartesianProduct<NoteChoice, NoteChoice, NoteChoice> _noteChoices;

    public TrioChordChoiceRepository(CompositionConfiguration configuration, INoteChoiceGenerator noteChoiceGenerator)
    {
        if (configuration.InstrumentConfigurations.Count != NumberOfInstruments)
        {
            throw new ArgumentException(
                "The composition configuration must contain exactly three instrument configurations.",
                nameof(configuration)
            );
        }

        var noteChoicesForInstruments = configuration.InstrumentConfigurations
            .OrderBy(static instrumentConfiguration => instrumentConfiguration.Instrument)
            .Select(instrumentConfiguration => noteChoiceGenerator.GenerateNoteChoices(instrumentConfiguration.Instrument))
            .ToList();

        _noteChoices = new LazyCartesianProduct<NoteChoice, NoteChoice, NoteChoice>(
            [.. noteChoicesForInstruments[0]],
            [.. noteChoicesForInstruments[1]],
            [.. noteChoicesForInstruments[2]]
        );
    }

    public BigInteger Count => _noteChoices.Size;

    public ChordChoice GetChordChoice(BigInteger id) => _noteChoices[id].ToChordChoice();

    public BigInteger GetChordChoiceId(ChordChoice chordChoice) => _noteChoices.IndexOf(
        (
            chordChoice.NoteChoices[0],
            chordChoice.NoteChoices[1],
            chordChoice.NoteChoices[2]
        )
    );
}
