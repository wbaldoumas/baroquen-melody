using BaroquenMelody.Library.Infrastructure.Configuration.Enums;
using BaroquenMelody.Library.Infrastructure.Configuration.Enums.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Infrastructure.Configurations.Enums.Extensions;

[TestFixture]
internal sealed class ConfigurationStatusExtensionsTests
{
    [Test]
    [TestCase(ConfigurationStatus.Enabled, ConfigurationStatus.Locked)]
    [TestCase(ConfigurationStatus.Locked, ConfigurationStatus.Disabled)]
    [TestCase(ConfigurationStatus.Disabled, ConfigurationStatus.Enabled)]
    public void Cycle_returns_the_next_value(ConfigurationStatus status, ConfigurationStatus expectedNextStatus)
    {
        // act
        var nextStatus = status.Cycle();

        // assert
        nextStatus.Should().Be(expectedNextStatus);
    }

    [Test]
    [TestCase(ConfigurationStatus.Enabled, true)]
    [TestCase(ConfigurationStatus.Locked, true)]
    [TestCase(ConfigurationStatus.Disabled, false)]
    public void IsEnabled_returns_whether_the_configuration_is_enabled(ConfigurationStatus status, bool expectedIsEnabled)
    {
        // act
        var isEnabled = status.IsEnabled();

        // assert
        isEnabled.Should().Be(expectedIsEnabled);
    }

    [Test]
    [TestCase(ConfigurationStatus.Enabled, false)]
    [TestCase(ConfigurationStatus.Locked, true)]
    [TestCase(ConfigurationStatus.Disabled, true)]
    public void IsFrozen_returns_whether_the_configuration_is_frozen(ConfigurationStatus status, bool expectedIsFrozen)
    {
        // act
        var isFrozen = status.IsFrozen();

        // assert
        isFrozen.Should().Be(expectedIsFrozen);
    }
}
