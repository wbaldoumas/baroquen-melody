namespace BaroquenMelody.Library.Midi;

public interface IMidiSaver
{
    /// <summary>
    ///     Saves the specified Baroquen melody to a temporary file and returns the path to the file.
    /// </summary>
    /// <param name="midiFileComposition">The Baroquen melody to save.</param>
    /// <param name="cancellationToken">A cancellation token to cooperatively cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the path to the saved file.</returns>
    Task<string> SaveTempAsync(MidiFileComposition midiFileComposition, CancellationToken cancellationToken);

    /// <summary>
    ///     Save the specified Baroquen melody to a file.
    /// </summary>
    /// <param name="midiFileComposition">The Baroquen melody to save.</param>
    /// <param name="tempPath">The path of the previously saved temporary file.</param>
    /// <param name="cancellationToken">A cancellation token to cooperatively cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<bool> SaveAsync(MidiFileComposition midiFileComposition, string tempPath, CancellationToken cancellationToken);
}
