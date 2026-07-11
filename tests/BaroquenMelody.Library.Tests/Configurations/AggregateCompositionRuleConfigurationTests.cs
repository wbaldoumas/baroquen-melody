using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Rules.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Configurations;

[TestFixture]
internal sealed class AggregateCompositionRuleConfigurationTests
{
    [Test]
    public void Default_ContainsAConfigurationForEveryCompositionRule()
    {
        // act
        var configuredRules = AggregateCompositionRuleConfiguration.Default.Configurations
            .Select(static configuration => configuration.Rule)
            .Order()
            .ToList();

        // assert
        configuredRules.Should().Equal(EnumUtils<CompositionRule>.AsEnumerable().Order());
    }

    [Test]
    public void Default_EnablesEveryCompositionRule()
    {
        // assert - the starting voicings are now validated and dead-ends are retriable, so no rule needs to be
        // opt-in; unsatisfiable voice spacing configurations are disabled dynamically by the composer configurator
        AggregateCompositionRuleConfiguration.Default.Configurations
            .Should().OnlyContain(static configuration => configuration.IsEnabled);
    }
}
