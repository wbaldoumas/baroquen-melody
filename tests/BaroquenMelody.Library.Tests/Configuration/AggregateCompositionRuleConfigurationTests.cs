using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Rules.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Configuration;

[TestFixture]
internal sealed class AggregateCompositionRuleConfigurationTests
{
    [Test]
    public void Default_ContainsEveryCompositionRule()
    {
        // act
        var configuredRules = AggregateCompositionRuleConfiguration.Default.Configurations
            .Select(static configuration => configuration.Rule)
            .Order();

        // assert
        configuredRules.Should().Equal(EnumUtils<CompositionRule>.AsEnumerable().Order());
    }

    [Test]
    public void Default_DisablesOnlyEnforceVoiceSpacing()
    {
        // assert: forward checking (Phase 8) removed the search-cost objection to EnforceVoiceSpacing — it composes at
        // no measurable cost — but the initial and fugal-entry voicings are not rule-checked, so enabling it by default
        // can start the composer in a spacing-unsatisfiable position and dead-end the whole composition. It stays
        // opt-in until those starting voicings are validated.
        AggregateCompositionRuleConfiguration.Default.Configurations
            .Should()
            .OnlyContain(static configuration =>
                configuration.Status == (configuration.Rule == CompositionRule.EnforceVoiceSpacing
                    ? ConfigurationStatus.Disabled
                    : ConfigurationStatus.Enabled));
    }
}
