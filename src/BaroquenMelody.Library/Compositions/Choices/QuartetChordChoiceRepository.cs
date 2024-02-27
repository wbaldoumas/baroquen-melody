﻿using BaroquenMelody.Library.Compositions.Choices.Extensions;
using BaroquenMelody.Library.Compositions.Configurations;
using LazyCart;
using System.Numerics;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <inheritdoc cref="IChordChoiceRepository"/>
internal sealed class QuartetChordChoiceRepository : IChordChoiceRepository
{
    public const int NumberOfVoices = 4;

    private readonly LazyCartesianProduct<NoteChoice, NoteChoice, NoteChoice, NoteChoice> _noteChoices;

    public QuartetChordChoiceRepository(
        CompositionConfiguration configuration,
        INoteChoiceGenerator noteChoiceGenerator)
    {
        if (configuration.VoiceConfigurations.Count != NumberOfVoices)
        {
            throw new ArgumentException(
                "The composition configuration must contain exactly four voice configurations.",
                nameof(configuration)
            );
        }

        var noteChoicesForVoices = configuration.VoiceConfigurations
            .OrderBy(voiceConfiguration => voiceConfiguration.Voice)
            .Select(voiceConfiguration => noteChoiceGenerator.GenerateNoteChoices(voiceConfiguration.Voice)).ToList();

        _noteChoices = new LazyCartesianProduct<NoteChoice, NoteChoice, NoteChoice, NoteChoice>(
            noteChoicesForVoices[0].ToList(),
            noteChoicesForVoices[1].ToList(),
            noteChoicesForVoices[2].ToList(),
            noteChoicesForVoices[3].ToList()
        );
    }

    public BigInteger Count => _noteChoices.Size;

    public ChordChoice GetChordChoice(BigInteger id) => _noteChoices[id].ToChordChoice();

    public BigInteger GetChordChoiceId(ChordChoice chordChoice) => _noteChoices.IndexOf(
        (
            chordChoice.NoteChoices[0],
            chordChoice.NoteChoices[1],
            chordChoice.NoteChoices[2],
            chordChoice.NoteChoices[3]
        )
    );
}
