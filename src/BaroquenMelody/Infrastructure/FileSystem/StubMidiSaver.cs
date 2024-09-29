using BaroquenMelody.Library.Compositions.Midi;

namespace BaroquenMelody.Infrastructure.FileSystem;

internal sealed class StubMidiSaver : IMidiSaver
{
    public Task<string> SaveTempAsync(Library.BaroquenMelody baroquenMelody, CancellationToken cancellationToken)
    {
        return Task.FromResult(string.Empty);
    }

    public Task<bool> SaveAsync(Library.BaroquenMelody baroquenMelody, string tempPath, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
