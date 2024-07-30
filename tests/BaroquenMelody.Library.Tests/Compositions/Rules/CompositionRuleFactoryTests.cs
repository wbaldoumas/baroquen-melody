using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Compositions.Rules.Enums;
using BaroquenMelody.Library.Infrastructure.Random;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Rules;

[TestFixture]
internal sealed class CompositionRuleFactoryTests
{
    private CompositionRuleFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.C3, Notes.C5),
                new(Voice.Alto, Notes.C2, Notes.C4)
            },
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        _factory = new CompositionRuleFactory(compositionConfiguration, Substitute.For<IWeightedRandomBooleanGenerator>());
    }

    [Test]
    [TestCase(ConfigurableCompositionRule.AvoidDissonance, int.MaxValue, typeof(AvoidDissonance))]
    [TestCase(ConfigurableCompositionRule.AvoidDissonantLeaps, int.MaxValue, typeof(AvoidDissonantLeaps))]
    [TestCase(ConfigurableCompositionRule.HandleAscendingSeventh, int.MaxValue, typeof(HandleAscendingSeventh))]
    [TestCase(ConfigurableCompositionRule.AvoidRepetition, int.MaxValue, typeof(AvoidRepetition))]
    [TestCase(ConfigurableCompositionRule.AvoidParallelFourths, int.MaxValue, typeof(AvoidParallelIntervals))]
    [TestCase(ConfigurableCompositionRule.AvoidParallelFifths, int.MaxValue, typeof(AvoidParallelIntervals))]
    [TestCase(ConfigurableCompositionRule.AvoidParallelOctaves, int.MaxValue, typeof(AvoidParallelIntervals))]
    [TestCase(ConfigurableCompositionRule.AvoidDirectFourths, int.MaxValue, typeof(AvoidDirectIntervals))]
    [TestCase(ConfigurableCompositionRule.AvoidDirectFifths, int.MaxValue, typeof(AvoidDirectIntervals))]
    [TestCase(ConfigurableCompositionRule.AvoidDirectOctaves, int.MaxValue, typeof(AvoidDirectIntervals))]
    [TestCase(ConfigurableCompositionRule.AvoidOverDoubling, int.MaxValue, typeof(AvoidOverDoubling))]
    [TestCase(ConfigurableCompositionRule.FollowStandardChordProgression, int.MaxValue, typeof(FollowsStandardProgression))]
    [TestCase(ConfigurableCompositionRule.AvoidDissonance, int.MinValue, typeof(CompositionRuleBypass))]
    public void Create_returns_expected_rule(ConfigurableCompositionRule rule, int strictness, Type expectedRuleType)
    {
        // arrange
        var configuration = new CompositionRuleConfiguration(rule, strictness);

        // act
        var result = _factory.Create(configuration);

        // assert
        result.Should().BeOfType(expectedRuleType);
    }

    [Test]
    public void Create_WhenCompositionRuleIsUnsupported_ThrowsArgumentOutOfRangeException()
    {
        // arrange
        var configuration = new CompositionRuleConfiguration((ConfigurableCompositionRule)byte.MaxValue);

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
