using BaroquenMelody.Library.Compositions.Configurations;
using LazyCart;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace BaroquenMelody.Library.Compositions.Contexts;

/// <inheritdoc cref="IChordContextRepository"/>
internal sealed class QuartetChordContextRepository : IChordContextRepository
{
    public const int NumberOfVoices = 4;

    private readonly ILazyCartesianProduct<NoteContext, NoteContext, NoteContext, NoteContext> _noteContexts;

    public QuartetChordContextRepository(
        CompositionConfiguration configuration,
        INoteContextGenerator noteContextGenerator)
    {
        if (configuration.VoiceConfigurations.Count != NumberOfVoices)
        {
            throw new ArgumentException(
                "The composition configuration must contain exactly four voice configurations.",
                nameof(configuration)
            );
        }

        var noteContextsForVoices = configuration.VoiceConfigurations
            .OrderBy(voiceConfiguration => voiceConfiguration.Voice)
            .Select(voiceConfiguration => noteContextGenerator.GenerateNoteContexts(voiceConfiguration, configuration.Scale))
            .ToList();

        _noteContexts = new LazyCartesianProduct<NoteContext, NoteContext, NoteContext, NoteContext>(
            noteContextsForVoices[0].ToList(),
            noteContextsForVoices[1].ToList(),
            noteContextsForVoices[2].ToList(),
            noteContextsForVoices[3].ToList()
        );
    }

    [ExcludeFromCodeCoverage]
    public BigInteger Count => _noteContexts.Size;

    public BigInteger GetChordContextIndex(ChordContext chordContext) =>
        _noteContexts.IndexOf(
            (
                chordContext.NoteContexts[0],
                chordContext.NoteContexts[1],
                chordContext.NoteContexts[2],
                chordContext.NoteContexts[3]
            )
        );
}
