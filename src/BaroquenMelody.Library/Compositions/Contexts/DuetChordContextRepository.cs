using BaroquenMelody.Library.Compositions.Configurations;
using LazyCart;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace BaroquenMelody.Library.Compositions.Contexts;

/// <inheritdoc cref="IChordContextRepository"/>
internal sealed class DuetChordContextRepository : IChordContextRepository
{
    public const int NumberOfVoices = 2;

    private readonly LazyCartesianProduct<NoteContext, NoteContext> _noteContexts;

    public DuetChordContextRepository(CompositionConfiguration configuration, INoteContextGenerator noteContextGenerator)
    {
        if (configuration.VoiceConfigurations.Count != NumberOfVoices)
        {
            throw new ArgumentException(
                "The composition configuration must contain exactly two voice configurations.",
                nameof(configuration)
            );
        }

        var noteContextsForVoices = configuration.VoiceConfigurations
            .OrderBy(voiceConfiguration => voiceConfiguration.Voice)
            .Select(voiceConfiguration => noteContextGenerator.GenerateNoteContexts(voiceConfiguration, configuration.Scale))
            .ToList();

        _noteContexts = new LazyCartesianProduct<NoteContext, NoteContext>(
            noteContextsForVoices[0].ToList(),
            noteContextsForVoices[1].ToList()
        );
    }

    [ExcludeFromCodeCoverage]
    public BigInteger Count => _noteContexts.Size;

    public BigInteger GetChordContextId(ChordContext chordContext) => _noteContexts.IndexOf((chordContext.NoteContexts[0], chordContext.NoteContexts[1]));
}
