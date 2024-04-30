using BaroquenMelody.Library.Compositions.Choices.Extensions;
using BaroquenMelody.Library.Compositions.Configurations;
using LazyCart;
using System.Numerics;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class DuetChordChoiceRepository : IChordChoiceRepository
{
    public const int NumberOfVoices = 2;

    private readonly LazyCartesianProduct<NoteChoice, NoteChoice> _noteChoices;

    public DuetChordChoiceRepository(CompositionConfiguration compositionConfiguration, INoteChoiceGenerator noteChoiceGenerator)
    {
        if (compositionConfiguration.VoiceConfigurations.Count != NumberOfVoices)
        {
            throw new ArgumentException(
                "The composition configuration must contain exactly two voice configurations.",
                nameof(compositionConfiguration)
            );
        }

        var noteChoicesForVoices = compositionConfiguration.VoiceConfigurations
            .OrderBy(voiceConfiguration => voiceConfiguration.Voice)
            .Select(voiceConfiguration => noteChoiceGenerator.GenerateNoteChoices(voiceConfiguration.Voice)).ToList();

        _noteChoices = new LazyCartesianProduct<NoteChoice, NoteChoice>(
            [.. noteChoicesForVoices[0]],
            [.. noteChoicesForVoices[1]]
        );
    }

    public BigInteger Count => _noteChoices.Size;

    public ChordChoice GetChordChoice(BigInteger id) => _noteChoices[id].ToChordChoice();

    public BigInteger GetChordChoiceId(ChordChoice chordChoice) => _noteChoices.IndexOf(
        (chordChoice.NoteChoices[0], chordChoice.NoteChoices[1])
    );
}
