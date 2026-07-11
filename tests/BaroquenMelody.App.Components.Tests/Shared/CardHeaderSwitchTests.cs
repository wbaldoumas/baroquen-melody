using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.Library.Configurations.Enums;
using Bunit;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class CardHeaderSwitchTests
{
    private AppComponentsTestContext _testContext = null!;

    private ConfigurationStatus? _reportedStatus;

    [SetUp]
    public void SetUp()
    {
        _testContext = new AppComponentsTestContext();
        _reportedStatus = null;
    }

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void HeaderContent_is_rendered()
    {
        // act
        var component = RenderCardHeaderSwitch(ConfigurationStatus.Enabled);

        // assert
        component.Find("#test-header-content").TextContent.Should().Be("test header");
    }

    [TestCase(ConfigurationStatus.Enabled, true, false, false)]
    [TestCase(ConfigurationStatus.Disabled, false, false, false)]
    [TestCase(ConfigurationStatus.EnabledAndLocked, true, true, true)]
    [TestCase(ConfigurationStatus.DisabledAndLocked, false, true, true)]
    public void Switches_render_the_configuration_status(ConfigurationStatus status, bool expectedEnableChecked, bool expectedLockChecked, bool expectedEnableSwitchDisabled)
    {
        // act
        var component = RenderCardHeaderSwitch(status);

        // assert
        component.EnableSwitch().IsChecked.Should().Be(expectedEnableChecked);
        component.LockSwitch().IsChecked.Should().Be(expectedLockChecked);
        component.EnableSwitch().IsDisabled.Should().Be(expectedEnableSwitchDisabled);
        component.LockSwitch().IsDisabled.Should().BeFalse();
    }

    [TestCase(ConfigurationStatus.Enabled, false, ConfigurationStatus.Disabled)]
    [TestCase(ConfigurationStatus.Disabled, true, ConfigurationStatus.Enabled)]
    public void Toggling_the_enable_switch_reports_the_expected_status(ConfigurationStatus initialStatus, bool isEnabled, ConfigurationStatus expectedStatus)
    {
        // arrange
        var component = RenderCardHeaderSwitch(initialStatus);

        // act
        component.EnableSwitch().Change(isEnabled);

        // assert
        _reportedStatus.Should().Be(expectedStatus);
    }

    [TestCase(ConfigurationStatus.Enabled, true, ConfigurationStatus.EnabledAndLocked)]
    [TestCase(ConfigurationStatus.Disabled, true, ConfigurationStatus.DisabledAndLocked)]
    [TestCase(ConfigurationStatus.EnabledAndLocked, false, ConfigurationStatus.Enabled)]
    [TestCase(ConfigurationStatus.DisabledAndLocked, false, ConfigurationStatus.Disabled)]
    public void Toggling_the_lock_switch_reports_the_expected_status(ConfigurationStatus initialStatus, bool isLocked, ConfigurationStatus expectedStatus)
    {
        // arrange
        var component = RenderCardHeaderSwitch(initialStatus);

        // act
        component.LockSwitch().Change(isLocked);

        // assert
        _reportedStatus.Should().Be(expectedStatus);
    }

    private IRenderedComponent<CardHeaderSwitch> RenderCardHeaderSwitch(ConfigurationStatus status) => _testContext.RenderComponent<CardHeaderSwitch>(parameters => parameters
        .Add(component => component.HeaderContent, "<span id=\"test-header-content\">test header</span>")
        .Add(component => component.ConfigurationStatus, status)
        .Add(component => component.ValueChanged, status => _reportedStatus = status)
    );
}
