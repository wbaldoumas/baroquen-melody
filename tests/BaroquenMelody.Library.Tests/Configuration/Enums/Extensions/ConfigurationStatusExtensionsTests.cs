using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Configurations.Enums.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Configuration.Enums.Extensions;

[TestFixture]
internal sealed class ConfigurationStatusExtensionsTests
{
    [Test]
    [TestCase(ConfigurationStatus.None, false)]
    [TestCase(ConfigurationStatus.Enabled, true)]
    [TestCase(ConfigurationStatus.Locked, false)]
    [TestCase(ConfigurationStatus.Disabled, false)]
    [TestCase(ConfigurationStatus.EnabledAndLocked, true)]
    [TestCase(ConfigurationStatus.DisabledAndLocked, false)]
    [TestCase(ConfigurationStatus.Enabled | ConfigurationStatus.Locked, true)]
    [TestCase(ConfigurationStatus.Disabled | ConfigurationStatus.Locked, false)]
    public void IsEnabled_returns_whether_the_configuration_is_enabled(ConfigurationStatus status, bool expectedIsEnabled)
    {
        // act
        var isEnabled = status.IsEnabled();

        // assert
        isEnabled.Should().Be(expectedIsEnabled);
    }

    [Test]
    [TestCase(ConfigurationStatus.None, true)]
    [TestCase(ConfigurationStatus.Enabled, false)]
    [TestCase(ConfigurationStatus.Locked, true)]
    [TestCase(ConfigurationStatus.Disabled, true)]
    [TestCase(ConfigurationStatus.EnabledAndLocked, true)]
    [TestCase(ConfigurationStatus.DisabledAndLocked, true)]
    [TestCase(ConfigurationStatus.Enabled | ConfigurationStatus.Locked, true)]
    [TestCase(ConfigurationStatus.Disabled | ConfigurationStatus.Locked, true)]
    public void IsFrozen_returns_whether_the_configuration_is_frozen(ConfigurationStatus status, bool expectedIsFrozen)
    {
        // act
        var isFrozen = status.IsFrozen();

        // assert
        isFrozen.Should().Be(expectedIsFrozen);
    }
}
