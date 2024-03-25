using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Evaluations.Rules;

[TestFixture]
internal sealed class AggregateCompositionRuleTests
{
    private ICompositionRule _mockCompositionRule1 = null!;

    private ICompositionRule _mockCompositionRule2 = null!;

    private AggregateCompositionRule aggregateCompositionRule = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionRule1 = Substitute.For<ICompositionRule>();
        _mockCompositionRule2 = Substitute.For<ICompositionRule>();

        aggregateCompositionRule = new AggregateCompositionRule([_mockCompositionRule1, _mockCompositionRule2]);
    }

    [Test]
    public void Evaluate_WhenAllRulesPass_ReturnsTrue()
    {
        _mockCompositionRule1.Evaluate(default!, default!).ReturnsForAnyArgs(true);
        _mockCompositionRule2.Evaluate(default!, default!).ReturnsForAnyArgs(true);

        var result = aggregateCompositionRule.Evaluate(default!, default!);

        result.Should().BeTrue();
    }

    [Test]
    public void Evaluate_WhenAnyRuleFails_ReturnsFalse()
    {
        _mockCompositionRule1.Evaluate(default!, default!).ReturnsForAnyArgs(true);
        _mockCompositionRule2.Evaluate(default!, default!).ReturnsForAnyArgs(false);

        var result = aggregateCompositionRule.Evaluate(default!, default!);

        result.Should().BeFalse();
    }

    [Test]
    public void Evaluate_WhenAllRulesFail_ReturnsFalse()
    {
        _mockCompositionRule1.Evaluate(default!, default!).ReturnsForAnyArgs(false);
        _mockCompositionRule2.Evaluate(default!, default!).ReturnsForAnyArgs(false);

        var result = aggregateCompositionRule.Evaluate(default!, default!);

        result.Should().BeFalse();
    }
}
