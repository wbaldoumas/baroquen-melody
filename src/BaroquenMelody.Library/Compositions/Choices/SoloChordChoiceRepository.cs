using BaroquenMelody.Library.Compositions.Choices.Extensions;
using BaroquenMelody.Library.Compositions.Configurations;
using System.Numerics;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class SoloChordChoiceRepository : IChordChoiceRepository
{
    public const int NumberOfInstruments = 1;

    private readonly List<NoteChoice> _noteChoices;

    public SoloChordChoiceRepository(CompositionConfiguration compositionConfiguration, INoteChoiceGenerator noteChoiceGenerator)
    {
        if (compositionConfiguration.Instruments.Count != NumberOfInstruments)
        {
            throw new ArgumentException(
                "The composition configuration must contain exactly one instrument configuration.",
                nameof(compositionConfiguration)
            );
        }

        _noteChoices = noteChoiceGenerator.GenerateNoteChoices(compositionConfiguration.Instruments[0]).ToList();
    }

    public BigInteger Count => _noteChoices.Count;

    public ChordChoice GetChordChoice(BigInteger id) => _noteChoices[(int)id].ToChordChoice();

    public BigInteger GetChordChoiceId(ChordChoice chordChoice) => _noteChoices.IndexOf(chordChoice.NoteChoices[0]);
}
