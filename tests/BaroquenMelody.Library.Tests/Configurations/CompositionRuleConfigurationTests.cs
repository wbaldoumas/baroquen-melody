using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Rules.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Configurations;

[TestFixture]
internal sealed class CompositionRuleConfigurationTests
{
    [Test]
    public void IsEnabledAndIsFrozen_ReflectTheStatus_AfterAWithExpression()
    {
        // arrange
        var enabledConfiguration = new CompositionRuleConfiguration(CompositionRule.EnforceVoiceSpacing, ConfigurationStatus.Enabled);

        // act - a with-expression clones the record rather than re-running the constructor, so the derived
        // properties must be computed from the status instead of being captured at construction time
        var disabledConfiguration = enabledConfiguration with { Status = ConfigurationStatus.DisabledAndLocked };

        // assert
        disabledConfiguration.IsEnabled.Should().BeFalse();
        disabledConfiguration.IsFrozen.Should().BeTrue();
        enabledConfiguration.IsEnabled.Should().BeTrue();
        enabledConfiguration.IsFrozen.Should().BeFalse();
    }
}
