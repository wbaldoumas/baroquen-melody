using BaroquenMelody.Library.Midi;

namespace BaroquenMelody.Infrastructure.FileSystem;

internal sealed class StubMidiLauncher : IMidiLauncher
{
    public Task LaunchAsync(string path, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
