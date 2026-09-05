using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Rules;
using Fluxor;
using Melanchall.DryWetMidi.Interaction;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BaroquenMelody.Library.Tests.TestData;

/// <summary>
///     Builds compositions from a fixed seed via a <see cref="SeededRandomProvider"/>, for the determinism,
///     invariant, and golden-master safety-net tests.
/// </summary>
internal static class SeededComposition
{
    public static MidiFileComposition Compose(CompositionConfiguration configuration, int seed)
    {
        var configurator = new BaroquenMelodyComposerConfigurator(
            Substitute.For<ILogger<MidiFileComposition>>(),
            Substitute.For<IDispatcher>(),
            SeededRandomProviders.ForComposition(seed),
            SeededRandomProviders.ForProcessorShuffle(seed),
            new VoiceSpacingSatisfiabilityAnalyzer()
        );

        return configurator.Configure(configuration).Compose(CancellationToken.None);
    }

    public static IReadOnlyList<MidiNoteSnapshot> Notes(MidiFileComposition composition) => composition.MidiFile
        .GetNotes()
        .Select(static note => new MidiNoteSnapshot((int)note.NoteNumber, (int)note.Velocity, note.Time, note.Length))
        .ToList();
}
