using BaroquenMelody.Library.Composition.Configurations;
using LazyCart;
using System.Numerics;

namespace BaroquenMelody.Library.Composition.Contexts;

/// <inheritdoc cref="IChordContextRepository"/>
internal sealed class TrioChordContextRepository : IChordContextRepository
{
    public const int NumberOfVoices = 3;

    private readonly ILazyCartesianProduct<NoteContext, NoteContext, NoteContext> _noteContexts;

    public TrioChordContextRepository(
        CompositionConfiguration configuration,
        INoteContextGenerator noteContextGenerator)
    {
        if (configuration.VoiceConfigurations.Count != NumberOfVoices)
        {
            throw new ArgumentException(
                "The composition configuration must contain exactly three voice configurations.",
                nameof(configuration)
            );
        }

        var noteContextsForVoices = configuration.VoiceConfigurations
            .OrderBy(voiceConfiguration => voiceConfiguration.Voice)
            .Select(noteContextGenerator.GenerateNoteContexts).ToList();

        _noteContexts = new LazyCartesianProduct<NoteContext, NoteContext, NoteContext>(
            noteContextsForVoices[0].ToList(),
            noteContextsForVoices[1].ToList(),
            noteContextsForVoices[2].ToList()
        );
    }

    public BigInteger GetChordContextId(ChordContext chordContext) =>
        _noteContexts.IndexOf(
            (
                chordContext.NoteContexts[0],
                chordContext.NoteContexts[1],
                chordContext.NoteContexts[2]
            )
        );
}
