namespace BaroquenMelody.Library.Compositions.Midi;

public interface IMidiLauncher
{
    /// <summary>
    ///     Launches the MIDI file at the specified path. This will open the MIDI file
    ///     using the default application for MIDI files on the user's system.
    /// </summary>
    /// <param name="path">The path to the MIDI file to launch.</param>
    /// <param name="cancellationToken">A cancellation token to cooperatively cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task LaunchAsync(string path, CancellationToken cancellationToken);
}
