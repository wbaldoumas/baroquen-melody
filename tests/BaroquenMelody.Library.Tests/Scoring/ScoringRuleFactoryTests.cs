using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Scoring;
using BaroquenMelody.Library.Scoring.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Scoring;

[TestFixture]
internal sealed class ScoringRuleFactoryTests
{
    private ScoringRuleFactory _scoringRuleFactory = null!;

    [SetUp]
    public void SetUp() => _scoringRuleFactory = new ScoringRuleFactory(TestCompositionConfigurations.Get(2));

    [Test]
    public void Create_handles_all_scoring_rules()
    {
        foreach (var scoringRule in EnumUtils<ScoringRule>.AsEnumerable())
        {
            // act
            var act = () => _scoringRuleFactory.Create(new ScoringRuleConfiguration(scoringRule, ConfigurationStatus.Enabled, Weight: 1));

            // assert
            act.Should().NotThrow();
        }
    }

    [Test]
    public void Create_throws_for_an_unsupported_scoring_rule()
    {
        // act
        var act = () => _scoringRuleFactory.Create(new ScoringRuleConfiguration((ScoringRule)byte.MaxValue, ConfigurationStatus.Enabled, Weight: 1));

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Create_applies_the_configured_weight()
    {
        // arrange: a single stepwise move costs one, so the weighted penalty equals the weight.
        var scoringRule = _scoringRuleFactory.Create(
            new ScoringRuleConfiguration(ScoringRule.PreferShortestVoiceMovement, ConfigurationStatus.Enabled, Weight: 2)
        );

        var nextChordWithOneStepwiseMove = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half)
        ]);

        // act
        var penalty = scoringRule.Score(StepwisePrecedingChords, nextChordWithOneStepwiseMove);

        // assert
        penalty.Should().Be(2d);
    }

    [Test]
    public void CreateAggregate_excludes_disabled_and_zero_weight_rules()
    {
        // arrange: both outer voices step upward, so shortest-movement costs 2 raw and similar motion costs 1 raw.
        var aggregateScoringRule = _scoringRuleFactory.CreateAggregate(
            new AggregateScoringRuleConfiguration(
                new HashSet<ScoringRuleConfiguration>
                {
                    new(ScoringRule.PreferShortestVoiceMovement, ConfigurationStatus.Enabled, Weight: 2),
                    new(ScoringRule.PreferContraryOuterVoiceMotion, ConfigurationStatus.Disabled, Weight: 5),
                    new(ScoringRule.PreferLeapRecovery, ConfigurationStatus.Enabled, Weight: 0)
                }
            )
        );

        // act
        var penalty = aggregateScoringRule.Score(StepwisePrecedingChords, StepwiseSimilarMotionNextChord);

        // assert
        penalty.Should().Be(4d, "only the enabled, positively-weighted shortest-movement rule should contribute (2 steps x weight 2)");
    }

    [Test]
    public void CreateAggregate_includes_enabled_rules()
    {
        // arrange
        var aggregateScoringRule = _scoringRuleFactory.CreateAggregate(
            new AggregateScoringRuleConfiguration(
                new HashSet<ScoringRuleConfiguration>
                {
                    new(ScoringRule.PreferShortestVoiceMovement, ConfigurationStatus.Enabled, Weight: 2),
                    new(ScoringRule.PreferContraryOuterVoiceMotion, ConfigurationStatus.Enabled, Weight: 5),
                    new(ScoringRule.PreferLeapRecovery, ConfigurationStatus.Enabled, Weight: 4)
                }
            )
        );

        // act
        var penalty = aggregateScoringRule.Score(StepwisePrecedingChords, StepwiseSimilarMotionNextChord);

        // assert
        penalty.Should().Be(9d, "shortest movement (2 steps x weight 2) and similar outer-voice motion (1 x weight 5) should both contribute");
    }

    private static List<BaroquenChord> StepwisePrecedingChords =>
    [
        new([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half)
        ])
    ];

    private static BaroquenChord StepwiseSimilarMotionNextChord => new([
        new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
    ]);
}
