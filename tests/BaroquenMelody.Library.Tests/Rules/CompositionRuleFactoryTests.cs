﻿using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Rules;

[TestFixture]
internal sealed class CompositionRuleFactoryTests
{
    private CompositionRuleFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        _factory = new CompositionRuleFactory(
            compositionConfiguration,
            Substitute.For<IWeightedRandomBooleanGenerator>(),
            Substitute.For<IChordNumberIdentifier>()
        );
    }

    [Test]
    [TestCase(CompositionRule.AvoidDissonance, int.MaxValue, typeof(AvoidDissonance))]
    [TestCase(CompositionRule.AvoidDissonantLeaps, int.MaxValue, typeof(AvoidDissonantLeaps))]
    [TestCase(CompositionRule.HandleAscendingSeventh, int.MaxValue, typeof(HandleAscendingSeventh))]
    [TestCase(CompositionRule.AvoidRepeatedNotes, int.MaxValue, typeof(AvoidRepetition))]
    [TestCase(CompositionRule.AvoidParallelFourths, int.MaxValue, typeof(AvoidParallelIntervals))]
    [TestCase(CompositionRule.AvoidParallelFifths, int.MaxValue, typeof(AvoidParallelIntervals))]
    [TestCase(CompositionRule.AvoidParallelOctaves, int.MaxValue, typeof(AvoidParallelIntervals))]
    [TestCase(CompositionRule.AvoidDirectFourths, int.MaxValue, typeof(AvoidDirectIntervals))]
    [TestCase(CompositionRule.AvoidDirectFifths, int.MaxValue, typeof(AvoidDirectIntervals))]
    [TestCase(CompositionRule.AvoidDirectOctaves, int.MaxValue, typeof(AvoidDirectIntervals))]
    [TestCase(CompositionRule.AvoidOverDoubling, int.MaxValue, typeof(AvoidOverDoubling))]
    [TestCase(CompositionRule.FollowStandardChordProgression, int.MaxValue, typeof(FollowsStandardProgression))]
    [TestCase(CompositionRule.AvoidRepeatedChords, int.MaxValue, typeof(AvoidRepeatedChords))]
    [TestCase(CompositionRule.AvoidDissonance, int.MinValue, typeof(CompositionRuleBypass))]
    public void Create_returns_expected_rule(CompositionRule rule, int strictness, Type expectedRuleType)
    {
        // arrange
        var configuration = new CompositionRuleConfiguration(rule, ConfigurationStatus.Enabled, strictness);

        // act
        var result = _factory.Create(configuration);

        // assert
        result.Should().BeOfType(expectedRuleType);
    }

    [Test]
    public void Create_WhenCompositionRuleIsUnsupported_ThrowsArgumentOutOfRangeException()
    {
        // arrange
        var configuration = new CompositionRuleConfiguration((CompositionRule)byte.MaxValue);

        // act
        var act = () => _factory.Create(configuration);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void CreateAggregate_returns_expected_rule()
    {
        // act
        var result = _factory.CreateAggregate(AggregateCompositionRuleConfiguration.Default);

        // assert
        result.Should().BeOfType<AggregateCompositionRule>();
    }
}
