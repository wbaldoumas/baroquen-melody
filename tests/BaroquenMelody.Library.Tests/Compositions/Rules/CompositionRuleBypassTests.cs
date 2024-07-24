using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Infrastructure.Random;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Rules;

[TestFixture]
internal sealed class CompositionRuleBypassTests
{
    private ICompositionRule _mockCompositionRule = null!;

    private IWeightedRandomBooleanGenerator _weightedRandomBooleanGenerator = null!;

    private static readonly IReadOnlyList<BaroquenChord> _precedingChords = [];

    private static readonly BaroquenChord _nextChord = new([]);

    [SetUp]
    public void SetUp()
    {
        _mockCompositionRule = Substitute.For<ICompositionRule>();
        _mockCompositionRule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(false);
        _weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator();
    }

    [Test]
    public void When_bypass_is_triggered_rule_is_not_evaluated()
    {
        // arrange
        const int strictness = 0;

        var bypass = new CompositionRuleBypass(_mockCompositionRule, _weightedRandomBooleanGenerator, strictness);

        // act
        var result = bypass.Evaluate(_precedingChords, _nextChord);

        // assert
        result.Should().BeTrue();

        _mockCompositionRule.DidNotReceiveWithAnyArgs().Evaluate(default!, default!);
    }

    [Test]
    public void When_bypass_is_not_triggered_rule_is_evaluated()
    {
        // arrange
        const int strictness = 100;

        var bypass = new CompositionRuleBypass(_mockCompositionRule, _weightedRandomBooleanGenerator, strictness);

        // act
        var result = bypass.Evaluate(_precedingChords, _nextChord);

        // assert
        result.Should().BeFalse();

        _mockCompositionRule.Received(1).Evaluate(_precedingChords, _nextChord);
    }
}
