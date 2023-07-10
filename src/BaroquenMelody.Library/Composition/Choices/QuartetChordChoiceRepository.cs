using BaroquenMelody.Library.Composition.Choices.Extensions;
using BaroquenMelody.Library.Composition.Configurations;
using LazyCart;
using System.Numerics;

namespace BaroquenMelody.Library.Composition.Choices;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class QuartetChordChoiceRepository : IChordChoiceRepository
{
    private const int ExpectedNumberOfVoices = 4;

    private readonly ILazyCartesianProduct<NoteChoice, NoteChoice, NoteChoice, NoteChoice> _noteChoices;

    public QuartetChordChoiceRepository(CompositionConfiguration configuration, INoteChoiceGenerator noteChoiceGenerator)
    {
        if (configuration.VoiceConfigurations.Count != ExpectedNumberOfVoices)
        {
            throw new ArgumentException(
                "The composition configuration must contain exactly four voice configurations.",
                nameof(configuration)
            );
        }

        var noteChoicesForVoices = configuration.VoiceConfigurations.Select(
            voiceConfiguration => noteChoiceGenerator.GenerateNoteChoices(voiceConfiguration.Voice)
        ).ToList();

        _noteChoices = new LazyCartesianProduct<NoteChoice, NoteChoice, NoteChoice, NoteChoice>(
            noteChoicesForVoices[0].ToList(),
            noteChoicesForVoices[1].ToList(),
            noteChoicesForVoices[2].ToList(),
            noteChoicesForVoices[3].ToList()
        );
    }

    public BigInteger Count => _noteChoices.Size;

    public ChordChoice GetChordChoice(BigInteger index) => _noteChoices[index].ToChordChoice();
}
