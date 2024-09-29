namespace BaroquenMelody.Library.Configurations.Enums.Extensions;

public static class ConfigurationStatusExtensions
{
    /// <summary>
    ///     Cycle the configuration status.
    /// </summary>
    /// <param name="status">The source configuration status.</param>
    /// <returns>The next configuration status.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the source configuration status is not a valid value.</exception>
    public static ConfigurationStatus Cycle(this ConfigurationStatus status) => status switch
    {
        ConfigurationStatus.Enabled => ConfigurationStatus.Locked,
        ConfigurationStatus.Locked => ConfigurationStatus.Disabled,
        ConfigurationStatus.Disabled => ConfigurationStatus.Enabled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    /// <summary>
    ///     Whether the configuration is enabled (enabled or locked).
    /// </summary>
    /// <param name="status">The source configuration status.</param>
    /// <returns>Whether the configuration is enabled.</returns>
    public static bool IsEnabled(this ConfigurationStatus status) => status is ConfigurationStatus.Enabled or ConfigurationStatus.Locked;

    /// <summary>
    ///     Whether the configuration is frozen (locked or disabled).
    /// </summary>
    /// <param name="status">The source configuration status.</param>
    /// <returns>Whether the configuration is frozen.</returns>
    public static bool IsFrozen(this ConfigurationStatus status) => status is ConfigurationStatus.Locked or ConfigurationStatus.Disabled;
}
