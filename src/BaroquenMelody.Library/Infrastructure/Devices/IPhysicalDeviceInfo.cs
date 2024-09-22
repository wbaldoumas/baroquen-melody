namespace BaroquenMelody.Library.Infrastructure.Devices;

/// <summary>
///     An abstraction for device information.
/// </summary>
public interface IPhysicalDeviceInfo
{
    /// <summary>
    ///     Whether the device is a mobile device.
    /// </summary>
    bool IsMobile { get; }
}
