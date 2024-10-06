using BaroquenMelody.Library.Midi;

namespace BaroquenMelody.Infrastructure.FileSystem;

internal sealed class StubMidiSaver : IMidiSaver
{
    public Task<string> SaveTempAsync(Library.MidiFileComposition midiFileComposition, CancellationToken cancellationToken)
    {
        return Task.FromResult(string.Empty);
    }

    public Task<bool> SaveAsync(Library.MidiFileComposition midiFileComposition, string tempPath, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
