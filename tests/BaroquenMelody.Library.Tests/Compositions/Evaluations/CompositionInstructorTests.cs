using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Evaluations;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Evaluations;

[TestFixture]
internal sealed class CompositionInstructorTests
{
    private ICompositionRule _mockCompositionRuleA = null!;

    private ICompositionRule _mockCompositionRuleB = null!;

    private CompositionInstructor _compositionInstructor = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionRuleA = Substitute.For<ICompositionRule>();
        _mockCompositionRuleB = Substitute.For<ICompositionRule>();

        _compositionInstructor = new CompositionInstructor(
        [
            _mockCompositionRuleA,
            _mockCompositionRuleB
        ]);
    }

    [Test]
    public void WhenEvaluateCompositionIsInvoked_ThenCompositionIsReturned()
    {
        // arrange
        var composition = new Composition([]);

        _mockCompositionRuleA.EvaluateComposition(composition).Returns(composition);
        _mockCompositionRuleB.EvaluateComposition(composition).Returns(composition);

        // act
        var evaluatedComposition = _compositionInstructor.EvaluateComposition(composition);

        // assert
        evaluatedComposition.Should().Be(composition);

        _mockCompositionRuleA.Received(1).EvaluateComposition(composition);
        _mockCompositionRuleB.Received(1).EvaluateComposition(composition);
    }
}
