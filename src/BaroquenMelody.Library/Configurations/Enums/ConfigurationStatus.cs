namespace BaroquenMelody.Library.Configurations.Enums;

[Flags]
public enum ConfigurationStatus : byte
{
    /// <summary>
    ///     No status.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The configuration is enabled.
    /// </summary>
    Enabled = 1 << 0, // 1

    /// <summary>
    ///     The configuration is disabled.
    /// </summary>
    Disabled = 1 << 1, // 2

    /// <summary>
    ///     The configuration is locked.
    /// </summary>
    Locked = 1 << 2, // 4

    /// <summary>
    ///     The configuration is enabled and locked.
    /// </summary>
    EnabledAndLocked = Enabled | Locked, // 1 | 4 = 5

    /// <summary>
    ///     The configuration is disabled and locked.
    /// </summary>
    DisabledAndLocked = Disabled | Locked // 2 | 4 = 6
}
