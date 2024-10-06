using BaroquenMelody.Library;
using BaroquenMelody.Library.Midi;

namespace BaroquenMelody.Infrastructure.FileSystem;

internal sealed class StubMidiSaver : IMidiSaver
{
    public Task<string> SaveTempAsync(MidiFileComposition midiFileComposition, CancellationToken cancellationToken)
    {
        return Task.FromResult(string.Empty);
    }

    public Task<bool> SaveAsync(MidiFileComposition midiFileComposition, string tempPath, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
