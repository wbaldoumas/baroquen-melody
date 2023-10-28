using BaroquenMelody.Library.Composition.Choices.Extensions;
using BaroquenMelody.Library.Composition.Configurations;
using LazyCart;
using System.Numerics;

namespace BaroquenMelody.Library.Composition.Choices;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class TrioChordChoiceRepository : IChordChoiceRepository
{
    public const int NumberOfVoices = 3;

    private readonly ILazyCartesianProduct<NoteChoice, NoteChoice, NoteChoice> _noteChoices;

    public TrioChordChoiceRepository(CompositionConfiguration configuration, INoteChoiceGenerator noteChoiceGenerator)
    {
        if (configuration.VoiceConfigurations.Count != NumberOfVoices)
        {
            throw new ArgumentException(
                "The composition configuration must contain exactly three voice configurations.",
                nameof(configuration)
            );
        }

        var noteChoicesForVoices = configuration.VoiceConfigurations
            .OrderBy(voiceConfiguration => voiceConfiguration.Voice)
            .Select(voiceConfiguration => noteChoiceGenerator.GenerateNoteChoices(voiceConfiguration.Voice)).ToList();

        _noteChoices = new LazyCartesianProduct<NoteChoice, NoteChoice, NoteChoice>(
            noteChoicesForVoices[0].ToList(),
            noteChoicesForVoices[1].ToList(),
            noteChoicesForVoices[2].ToList()
        );
    }

    public BigInteger Count => _noteChoices.Size;

    public ChordChoice GetChordChoice(BigInteger index) => _noteChoices[index].ToChordChoice();

    public BigInteger GetChordChoiceIndex(ChordChoice chordChoice) => _noteChoices.IndexOf(
        (
            chordChoice.NoteChoices[0],
            chordChoice.NoteChoices[1],
            chordChoice.NoteChoices[2]
        )
    );
}
