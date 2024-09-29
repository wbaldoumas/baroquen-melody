namespace BaroquenMelody.Library.Configurations.Services;

/// <summary>
///     Represents a service that persists and retrieves composition configurations.
/// </summary>
public interface ICompositionConfigurationPersistenceService
{
    /// <summary>
    ///     Saves the composition configuration.
    /// </summary>
    /// <param name="compositionConfiguration">The composition configuration to save.</param>
    /// <param name="name">The name of the composition configuration.</param>
    /// <param name="cancellationToken">A cancellation token to cooperatively cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<bool> SaveConfigurationAsync(CompositionConfiguration compositionConfiguration, string name, CancellationToken cancellationToken);

    /// <summary>
    ///     Loads all saved composition configurations.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cooperatively cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the saved composition configurations.</returns>
    Task<IEnumerable<SavedCompositionConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes the composition configuration with the specified name.
    /// </summary>
    /// <param name="name">The name of the composition configuration to delete.</param>
    /// <param name="cancellationToken">A cancellation token to cooperatively cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<bool> DeleteConfigurationAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    ///     Checks if a composition configuration with the specified name exists.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="cancellationToken">A cancellation token to cooperatively cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating whether the configuration exists.</returns>
    Task<bool> DoesConfigurationExist(string name, CancellationToken cancellationToken);
}
