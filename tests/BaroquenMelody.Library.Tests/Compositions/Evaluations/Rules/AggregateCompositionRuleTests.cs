using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Domain;
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
    public void Evaluate_AllRulesPass_ReturnsTrue()
    {
        // arrange
        var currentChord = new ContextualizedChord(new HashSet<ContextualizedNote>(), ChordContext.Empty, ChordChoice.Empty);
        var nextChord = new ContextualizedChord(new HashSet<ContextualizedNote>(), ChordContext.Empty, ChordChoice.Empty);

        _mockCompositionRule1.Evaluate(currentChord, nextChord).Returns(true);
        _mockCompositionRule2.Evaluate(currentChord, nextChord).Returns(true);

        // act
        var result = aggregateCompositionRule.Evaluate(currentChord, nextChord);

        // assert
        result.Should().BeTrue();
    }

    [Test]
    public void Evaluate_OneRuleFails_ReturnsFalse()
    {
        // arrange
        var currentChord = new ContextualizedChord(new HashSet<ContextualizedNote>(), ChordContext.Empty, ChordChoice.Empty);
        var nextChord = new ContextualizedChord(new HashSet<ContextualizedNote>(), ChordContext.Empty, ChordChoice.Empty);

        _mockCompositionRule1.Evaluate(currentChord, nextChord).Returns(true);
        _mockCompositionRule2.Evaluate(currentChord, nextChord).Returns(false);

        // act
        var result = aggregateCompositionRule.Evaluate(currentChord, nextChord);

        // assert
        result.Should().BeFalse();
    }
}
