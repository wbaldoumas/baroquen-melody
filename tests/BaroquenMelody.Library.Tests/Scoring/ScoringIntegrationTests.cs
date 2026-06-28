using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Scoring;

[TestFixture]
internal sealed class ScoringIntegrationTests
{
    private const int Seed = 42;

    [Test]
    public void Weighted_scoring_changes_the_composed_output()
    {
        // arrange: identical seeds; the only difference is whether any scoring rules are enabled.
        var configuration = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false };

        var scoringDisabledConfiguration = configuration with
        {
            AggregateScoringRuleConfiguration = new AggregateScoringRuleConfiguration(new HashSet<ScoringRuleConfiguration>())
        };

        // act
        var defaultNotes = SeededComposition.Notes(SeededComposition.Compose(configuration, Seed));
        var scoringDisabledNotes = SeededComposition.Notes(SeededComposition.Compose(scoringDisabledConfiguration, Seed));

        // assert: default-on weighted scoring must actually steer selection away from the legacy uniform random pick.
        defaultNotes.SequenceEqual(scoringDisabledNotes).Should().BeFalse();
    }

    [Test]
    public void Weighted_scoring_is_deterministic_under_a_seed()
    {
        // arrange: scoring rules enabled by default; same seed twice.
        var configuration = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false };

        // act
        var firstNotes = SeededComposition.Notes(SeededComposition.Compose(configuration, Seed));
        var secondNotes = SeededComposition.Notes(SeededComposition.Compose(configuration, Seed));

        // assert
        firstNotes.SequenceEqual(secondNotes).Should().BeTrue();
    }
}
