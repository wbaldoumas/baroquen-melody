using BaroquenMelody.Infrastructure.Devices;

namespace BaroquenMelody.App.Infrastructure.Devices;

internal sealed class MauiDeviceInfo : IPhysicalDeviceInfo
{
    public bool IsMobile => DeviceInfo.Current.Platform == DevicePlatform.Android || DeviceInfo.Current.Platform == DevicePlatform.iOS;
}
