namespace BaroquenMelody.Library.Configurations.Enums.Extensions;

public static class ConfigurationStatusExtensions
{
    /// <summary>
    ///     Whether the configuration is enabled (enabled or locked).
    /// </summary>
    /// <param name="status">The source configuration status.</param>
    /// <returns>Whether the configuration is enabled.</returns>
    public static bool IsEnabled(this ConfigurationStatus status) => status.HasFlag(ConfigurationStatus.Enabled);

    /// <summary>
    ///     Whether the configuration is frozen (locked or disabled).
    /// </summary>
    /// <param name="status">The source configuration status.</param>
    /// <returns>Whether the configuration is frozen.</returns>
    public static bool IsFrozen(this ConfigurationStatus status) => status.HasFlag(ConfigurationStatus.Locked) || status.HasFlag(ConfigurationStatus.Disabled)
                                                                                                               || status.HasFlag(ConfigurationStatus.DisabledAndLocked)
                                                                                                               || status.HasFlag(ConfigurationStatus.EnabledAndLocked)
                                                                                                               || (status.HasFlag(ConfigurationStatus.None) &&
                                                                                                                   !status.HasFlag(ConfigurationStatus.Enabled));
}
