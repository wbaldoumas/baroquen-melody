using BaroquenMelody.Library.Compositions.Choices.Extensions;
using BaroquenMelody.Library.Compositions.Configurations;
using LazyCart;
using System.Numerics;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class DuetChordChoiceRepository : IChordChoiceRepository
{
    public const int NumberOfInstruments = 2;

    private readonly LazyCartesianProduct<NoteChoice, NoteChoice> _noteChoices;

    public DuetChordChoiceRepository(CompositionConfiguration compositionConfiguration, INoteChoiceGenerator noteChoiceGenerator)
    {
        if (compositionConfiguration.Instruments.Count != NumberOfInstruments)
        {
            throw new ArgumentException(
                "The composition configuration must contain exactly two instrument configurations.",
                nameof(compositionConfiguration)
            );
        }

        var noteChoicesForInstruments = compositionConfiguration.Instruments
            .OrderBy(static instrument => instrument)
            .Select(noteChoiceGenerator.GenerateNoteChoices)
            .ToList();

        _noteChoices = new LazyCartesianProduct<NoteChoice, NoteChoice>(
            [.. noteChoicesForInstruments[0]],
            [.. noteChoicesForInstruments[1]]
        );
    }

    public BigInteger Count => _noteChoices.Size;

    public ChordChoice GetChordChoice(BigInteger id) => _noteChoices[id].ToChordChoice();

    public BigInteger GetChordChoiceId(ChordChoice chordChoice) => _noteChoices.IndexOf(
        (chordChoice.NoteChoices[0], chordChoice.NoteChoices[1])
    );
}
