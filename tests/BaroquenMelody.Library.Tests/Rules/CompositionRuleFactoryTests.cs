using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Rules;

[TestFixture]
internal sealed class CompositionRuleFactoryTests
{
    private IWeightedRandomBooleanGenerator _mockWeightedRandomBooleanGenerator = null!;

    private CompositionRuleFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        _mockWeightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();

        _factory = new CompositionRuleFactory(
            compositionConfiguration,
            _mockWeightedRandomBooleanGenerator,
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
    [TestCase(CompositionRule.AvoidVoiceCrossing, int.MaxValue, typeof(AvoidVoiceCrossing))]
    [TestCase(CompositionRule.AvoidVoiceOverlap, int.MaxValue, typeof(AvoidVoiceOverlap))]
    [TestCase(CompositionRule.EnforceVoiceSpacing, int.MaxValue, typeof(EnforceVoiceSpacing))]
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

    [Test]
    public void CreateAggregate_RejectsAnOutOfRangeChordBeforeAnyBypassableRuleCanDrawRandomness()
    {
        // arrange: every configured rule is minimally strict, so each is wrapped in a randomness-drawing bypass. The
        // range gate is prepended outside any bypass, so an out-of-range chord must fail before a single draw happens.
        // Forward checking in the chord-choice enumerator relies on this to prune out-of-range candidates without
        // shifting the seeded random stream; if this test fails, that pruning is no longer output-preserving.
        var laxConfigurations = AggregateCompositionRuleConfiguration.Default.Configurations
            .Select(static configuration => configuration with { Strictness = 0 })
            .ToHashSet();

        var aggregate = _factory.CreateAggregate(new AggregateCompositionRuleConfiguration(laxConfigurations));

        var precedingChords = new List<BaroquenChord>
        {
            new([
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
            ])
        };

        // C7 is above Instrument.One's C6 ceiling in the two-instrument test configuration.
        var outOfRangeChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C7, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        ]);

        // act
        var result = aggregate.Evaluate(precedingChords, outOfRangeChord);

        // assert
        result.Should().BeFalse("an out-of-range chord can never pass, no matter how lax the configured rules are");
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }
}
